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
    builder.Services.AddHttpContextAccessor();

    // RabbitMQ consumer
    builder.Services.AddSingleton<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>();
    builder.Services.AddHostedService<RabbitMqDemoConsumerService>();


    // TickerQ setup
    builder.Services.AddTickerQ(options =>
    {
        options.SetMaxConcurrency(2);
        options.AddOperationalStore<ApplicationDbContext>(efOpt =>
        {
            efOpt.CancelMissedTickersOnApplicationRestart();
            efOpt.UseModelCustomizerForMigrations();
        });
        options.AddDashboard();
        options.AddDashboardBasicAuth();
    });

    var app = builder.Build();
    app.Configuration["TickerQBasicAuth:Username"] = builder.Configuration["TickerQBasicAuth:Username"];
    app.Configuration["TickerQBasicAuth:Password"] = builder.Configuration["TickerQBasicAuth:Password"];
    app.UseTickerQ();
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