using BlogApp.Application.Services;
using BlogApp.Domain.Entities;
using BlogApp.Infrastructure.Data;
using BlogApp.Infrastructure.Data.Seeders;
using BlogApp.Infrastructure.Helpers;
using BlogApp.Infrastructure.Interceptors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace BlogApp.Seeder;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        Console.WriteLine("Starting App " + DateTime.Now);

        // Load secrets from Vault into configuration
        var secrets = await VaultHelper.LoadSecretsFromVaultAsync("BLOG_APP");
        builder.Configuration.ConfigureSecretsInConfiguration(secrets);

        ConfigureSerilog(builder);

        try
        {
            Log.Debug("Starting BlogApp Seeder");

            // Add DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionString = builder.Configuration["DBConnection"];
                if (string.IsNullOrEmpty(connectionString))
                {
                    Log.Error("DBConnection is not configured. Please check your configuration.");
                    return;
                }

                options.UseNpgsql(connectionString);
            });

            // Add required services
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            ConfigureRepositories(builder);

            var host = builder.Build();

            Log.Debug("Starting database seeding...");

            await DatabaseSeeder.SeedDatabaseAsync(host);
            Log.Information("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An error occurred while seeding the database");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static void ConfigureSerilog(WebApplicationBuilder builder)
    {
        // Get the Serilog configuration section
        var serilogConfig = builder.Configuration.GetSection("Serilog");
        var minimumLevelConfig = serilogConfig.GetSection("MinimumLevel");

        // Get the global log level from Serilog section, fallback to Information if not set
        var globalLogLevel = GetLogEventLevel(serilogConfig["GlobalLogLevel"] ?? "Information");

        // Get the default log level, fallback to global log level if not set
        var defaultLevel = GetLogEventLevel(minimumLevelConfig["Default"] ?? serilogConfig["GlobalLogLevel"] ?? "Information");

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(defaultLevel)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");

        // Apply overrides from configuration if they exist
        ApplyLogLevelOverride(loggerConfig, "Microsoft", minimumLevelConfig["Microsoft"], globalLogLevel);
        ApplyLogLevelOverride(loggerConfig, "Microsoft.AspNetCore", minimumLevelConfig["Microsoft.AspNetCore"], globalLogLevel);
        ApplyLogLevelOverride(loggerConfig, "Microsoft.EntityFrameworkCore", minimumLevelConfig["Microsoft.EntityFrameworkCore"], globalLogLevel);

        Log.Logger = loggerConfig.CreateLogger();
        builder.Logging.AddSerilog();
    }

    private static void ApplyLogLevelOverride(LoggerConfiguration loggerConfig, string source, string? level, LogEventLevel defaultLevel)
    {
        var logLevel = string.IsNullOrEmpty(level) ? defaultLevel : GetLogEventLevel(level);
        loggerConfig.MinimumLevel.Override(source, logLevel);
    }

    private static LogEventLevel GetLogEventLevel(string level)
    {
        return level.ToLower() switch
        {
            "verbose" => LogEventLevel.Verbose,
            "debug" => LogEventLevel.Debug,
            "information" => LogEventLevel.Information,
            "warning" => LogEventLevel.Warning,
            "error" => LogEventLevel.Error,
            "fatal" => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }

    private static void ConfigureRepositories(WebApplicationBuilder builder)
    {
        // Repositories and core services

        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        builder.Services.AddScoped<AuditableEntitySaveChangesInterceptor>();
        builder.Services.AddScoped<AuditLoggingInterceptor>();
    }
}