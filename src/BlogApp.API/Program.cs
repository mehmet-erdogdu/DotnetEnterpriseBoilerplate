using BlogApp.Application.Common.Behaviors;

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine("Starting App " + DateTime.Now);

// Load secrets from Vault into configuration
var secrets = await VaultHelper.LoadSecretsFromVaultAsync("BLOG_APP");
builder.Configuration.ConfigureSecretsInConfiguration(secrets);

builder.ConfigureSerilog();

try
{
    Log.Information("Starting BlogApp API");

    // Configure Kestrel
    ConfigureKestrel(builder);

    // Configure services
    ConfigureControllers(builder);
    ConfigureValidation(builder);
    ConfigureCors(builder);
    ConfigureForwardedHeaders(builder);
    ConfigureRateLimiting(builder);
    ConfigureLocalization(builder);
    ConfigureSwagger(builder);
    ConfigureDatabase(builder);
    ConfigureIdentity(builder);
    ConfigureAuthentication(builder);
    ConfigureMediatR(builder);
    ConfigureCaching(builder);
    ConfigureS3(builder);
    ConfigureRepositories(builder);
    ConfigureExternalServices(builder);

    var app = builder.Build();

    // Configure pipeline
    ConfigurePipeline(app, builder);

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

// Configuration methods
static void ConfigureKestrel(WebApplicationBuilder builder)
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.AddServerHeader = false;
        options.Limits.MaxRequestBodySize = 1_048_576; // 1 MB
        options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(10);
        options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(60);
    });
}

static void ConfigureControllers(WebApplicationBuilder builder)
{
    builder.Services.AddControllers(options =>
        {
            options.Filters.Add<ProblemDetailsResultFilter>();
            options.Filters.Add<SuccessContentTypeFilter>();
        })
        .ConfigureApiBehaviorOptions(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
            options.InvalidModelStateResponseFactory = CreateValidationErrorResponse;
        });
}

static IActionResult CreateValidationErrorResponse(ActionContext context)
{
    var messageService = context.HttpContext.RequestServices.GetRequiredService<IMessageService>();
    var details = context.ModelState
        .Where(kvp => kvp.Value is { Errors.Count: > 0 })
        .ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value!.Errors
                .Select(e => messageService.GetMessage(e.ErrorMessage))
                .Distinct()
                .ToArray()
        );

    var apiResponse = ApiResponse<object>.Failure(messageService.GetMessage("ValidationError"));
    apiResponse.Error!.Code = "VALIDATION_ERROR";

    // Combine validation details with correlation/trace info
    var enhancedDetails = new Dictionary<string, object>
    {
        ["correlationId"] = context.HttpContext.Response.Headers["X-Correlation-ID"].ToString(),
        ["traceId"] = context.HttpContext.TraceIdentifier,
        ["validationErrors"] = details
    };

    apiResponse.Error.Details = enhancedDetails;

    return new ObjectResult(apiResponse)
    {
        StatusCode = StatusCodes.Status400BadRequest,
        DeclaredType = typeof(ApiResponse<object>)
    };
}

static void ConfigureValidation(WebApplicationBuilder builder)
{
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssembly(typeof(CreatePostCommand).Assembly);
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
}

static void ConfigureCors(WebApplicationBuilder builder)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("SecureCorsPolicy", policy =>
        {
            var origins = (builder.Configuration["FrontendUrls"] ?? builder.Configuration["APIUrl"])
                !.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            policy
                .WithOrigins(origins)
                .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                .WithHeaders("Authorization", "Content-Type", "Cache-Control", "Pragma")
                .AllowCredentials()
                .WithExposedHeaders("X-Correlation-ID")
                .SetPreflightMaxAge(TimeSpan.FromHours(1));
        });
    });
}

static void ConfigureForwardedHeaders(WebApplicationBuilder builder)
{
    builder.Services.Configure<ForwardedHeadersOptions>(opts =>
    {
        opts.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        var knownProxiesCsv = builder.Configuration["KnownProxies"];
        if (string.IsNullOrWhiteSpace(knownProxiesCsv)) return;

        foreach (var ip in knownProxiesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            if (IPAddress.TryParse(ip, out var address))
                opts.KnownProxies.Add(address);
    });
}

static void ConfigureRateLimiting(WebApplicationBuilder builder)
{
    builder.Services.AddRateLimiter(options =>
    {
        // Global rate limiting
        options.AddFixedWindowLimiter("GlobalLimiter", limiterOptions =>
        {
            limiterOptions.PermitLimit = int.Parse(builder.Configuration["RateLimiting:GlobalLimit"]!);
            limiterOptions.Window = TimeSpan.FromMinutes(int.Parse(builder.Configuration["RateLimiting:WindowMinutes"]!));
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = int.Parse(builder.Configuration["RateLimiting:QueueLimit"]!);
        });

        // Authentication endpoints rate limiting
        options.AddFixedWindowLimiter("AuthLimiter", limiterOptions =>
        {
            limiterOptions.PermitLimit = int.Parse(builder.Configuration["RateLimiting:AuthLimit"]!);
            limiterOptions.Window = TimeSpan.FromMinutes(int.Parse(builder.Configuration["RateLimiting:AuthWindowMinutes"]!));
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 1;
        });

        options.OnRejected = async (context, token) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.HttpContext.Response.ContentType = "application/json";

            var messageService = context.HttpContext.RequestServices.GetRequiredService<IMessageService>();
            var apiResponse = ApiResponse<object>.Failure(messageService.GetMessage("TooManyRequests"));
            apiResponse.Error!.Code = "RATE_LIMIT_EXCEEDED";
            apiResponse.Error.Details = new Dictionary<string, object>
            {
                ["correlationId"] = context.HttpContext.Response.Headers["X-Correlation-ID"].ToString(),
                ["traceId"] = context.HttpContext.TraceIdentifier,
                ["retryAfter"] = "60 seconds"
            };

            var result = JsonSerializer.Serialize(apiResponse);
            await context.HttpContext.Response.WriteAsync(result, token);
        };

        // Apply global limiter policy by default
        options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
            PartitionedRateLimiter.Create<HttpContext, string>(_ =>
                RateLimitPartition.GetFixedWindowLimiter("global", _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = int.Parse(builder.Configuration["RateLimiting:GlobalLimit"]!),
                    QueueLimit = int.Parse(builder.Configuration["RateLimiting:QueueLimit"]!),
                    Window = TimeSpan.FromMinutes(int.Parse(builder.Configuration["RateLimiting:WindowMinutes"]!)),
                    AutoReplenishment = true
                }))
        );
    });
}

static void ConfigureLocalization(WebApplicationBuilder builder)
{
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IMessageService, MessageService>();
}

static void ConfigureSwagger(WebApplicationBuilder builder)
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    builder.Services.AddEndpointsApiExplorer();

    if (!builder.Environment.IsDevelopment()) return;

    builder.Services.AddSwaggerGen(option =>
    {
        option.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Blog API",
            Version = "v1",
            Description = "A simple blog API with JWT authentication"
        });

        if (File.Exists(xmlPath)) option.IncludeXmlComments(xmlPath);

        option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description =
                "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        option.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });
}

static void ConfigureDatabase(WebApplicationBuilder builder)
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(builder.Configuration["DBConnection"]));
}

static void ConfigureIdentity(WebApplicationBuilder builder)
{
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // Enhanced Password Policy
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 12;
            options.Password.RequiredUniqueChars = 1;

            // Enhanced Lockout Policy
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(int.Parse(builder.Configuration["Security:LockoutDurationMinutes"]!));
            options.Lockout.MaxFailedAccessAttempts = int.Parse(builder.Configuration["Security:MaxLoginAttempts"]!);
            options.Lockout.AllowedForNewUsers = true;

            // User Policy
            options.User.RequireUniqueEmail = true;
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

            // SignIn Policy - More secure
            options.SignIn.RequireConfirmedEmail = builder.Environment.IsProduction();
            options.SignIn.RequireConfirmedPhoneNumber = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    builder.Services.AddScoped<IPasswordValidator<ApplicationUser>, CustomPasswordValidator<ApplicationUser>>();
}

static void ConfigureAuthentication(WebApplicationBuilder builder)
{
    builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = builder.Environment.IsProduction();
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ClockSkew = TimeSpan.Zero,
                ValidAudience = builder.Configuration["JWT:ValidAudience"],
                ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]!))
            };
        });
}

static void ConfigureMediatR(WebApplicationBuilder builder)
{
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreatePostCommand).Assembly));
}

static void ConfigureCaching(WebApplicationBuilder builder)
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration["RedisConnection"];
        options.InstanceName = "BlogApp_";
    });

    builder.Services.AddScoped<ICacheService, RedisCacheService>();
    builder.Services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();
}

static void ConfigureS3(WebApplicationBuilder builder)
{
    builder.Services.AddSingleton<IAmazonS3>(_ =>
    {
        var clientConfig = new AmazonS3Config
        {
            ServiceURL = builder.Configuration["S3:Url"],
            ForcePathStyle = true,
            AuthenticationRegion = "us-east-1",
            UseHttp = true
        };

        return new AmazonS3Client(
            builder.Configuration["S3:AccessKeyId"],
            builder.Configuration["S3:SecretAccessKey"],
            clientConfig
        );
    });
}

static void ConfigureRepositories(WebApplicationBuilder builder)
{
    // Repositories and core services
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
    builder.Services.AddScoped<AuditableEntitySaveChangesInterceptor>();
    builder.Services.AddScoped<AuditLoggingInterceptor>();
    builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
    builder.Services.AddScoped<IPasswordHistoryRepository, PasswordHistoryRepository>();
    builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
    builder.Services.AddScoped<IPostRepository, PostRepository>();
    builder.Services.AddScoped<ITodoRepository, TodoRepository>();
    builder.Services.AddScoped<IFileRepository, FileRepository>();
    builder.Services.AddScoped<IPasswordService, PasswordService>();
    builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
    builder.Services.AddScoped<IFileService, MinIOService>();
    builder.Services.AddScoped<IFileValidationService, FileValidationService>();
}

static void ConfigureExternalServices(WebApplicationBuilder builder)
{
    // RabbitMQ
    builder.Services.AddSingleton<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>();
    builder.Services.AddScoped<IRabbitMqPublisher, RabbitMqPublisher>();

    // Firebase Notification Service
    builder.Services.AddScoped<IFirebaseNotificationService, FirebaseNotificationService>();
}

static void ConfigurePipeline(WebApplication app, WebApplicationBuilder builder)
{
    // Development pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        // Redirect root to Swagger in development
        app.MapGet("/", () => Results.Redirect("/swagger"));
    }

    if (!app.Environment.IsDevelopment())
        app.UseHsts();

    // Security and middleware pipeline
    if (bool.Parse(builder.Configuration["Security:RequireHttps"] ?? "false"))
        app.UseHttpsRedirection();

    app.UseCors("SecureCorsPolicy");
    app.UseSecurityHeaders();
    app.UseEnhancedSecurity();
    app.UseForwardedHeaders();
    app.UseRateLimiter();
    app.UseInputValidation();
    app.UseAntiForgery();
    app.UseNoCacheForAuth();
    app.UseCorrelationIdMiddleware();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRequestLogging();
    app.MapControllers();
    app.UseGlobalExceptionMiddleware();
}