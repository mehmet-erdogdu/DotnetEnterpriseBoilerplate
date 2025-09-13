[assembly: ExcludeFromCodeCoverage]


var builder = WebApplication.CreateBuilder(args);
Console.WriteLine("Starting App " + DateTime.Now);

// Load secrets from Vault into configuration
var secrets = await VaultHelper.LoadSecretsFromVaultAsync("BLOG_APP");
builder.Configuration.ConfigureSecretsInConfiguration(secrets);

builder.ConfigureSerilog();

try
{
    Log.Information("Starting BlogApp Worker");

    // DbContext - PostgreSQL
    builder.Services.AddDbContext<ApplicationDbContext>(options => { options.UseNpgsql(builder.Configuration["DBConnection"]); });

    // Infrastructure dependencies needed by the job
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
    builder.Services.AddScoped<AuditableEntitySaveChangesInterceptor>();
    builder.Services.AddScoped<AuditLoggingInterceptor>();
    builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
    builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
    builder.Services.AddScoped<RefreshTokenCleanupJobs>();
    builder.Services.AddHttpContextAccessor();

    // RabbitMQ consumer
    builder.Services.AddSingleton<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>();
    builder.Services.AddHostedService<RabbitMqDemoConsumerService>();

    // Hangfire setup
    builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(options => { options.UseNpgsqlConnection(builder.Configuration["DBConnection"]); }, new PostgreSqlStorageOptions
        {
            SchemaName = "hangfire"
        }));

    builder.Services.AddHangfireServer();

    var app = builder.Build();

    // Configure Hangfire Dashboard with authentication and dark mode
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = [new HangfireAuthorizationFilter()],
        DashboardTitle = "BlogApp Job Dashboard",
        DarkModeEnabled = true
    });

    // Register recurring jobs
    using (var scope = app.Services.CreateScope())
    {
        var refreshTokenCleanupJobs = scope.ServiceProvider.GetRequiredService<RefreshTokenCleanupJobs>();
        RecurringJob.AddOrUpdate("RefreshTokenCleanup", () => refreshTokenCleanupJobs.Run(), Cron.Daily);
    }

    // Add a simple health check endpoint
    app.MapGet("/", () => "BlogApp Worker is running. Access Hangfire Dashboard at /hangfire");

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