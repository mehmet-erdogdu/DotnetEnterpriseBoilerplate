using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace BlogApp.Infrastructure.Helpers;

[ExcludeFromCodeCoverage]
public static class SerilogHelper
{
    public static void ConfigureSerilog(this WebApplicationBuilder builder)
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
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(builder.Configuration["ElasticUrl"]!))
            {
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                IndexFormat = $"blogapp-logs-{DateTime.UtcNow:yyyy-MM}",
                NumberOfReplicas = 0,
                NumberOfShards = 1,
                BufferFileSizeLimitBytes = 5242880,
                Period = TimeSpan.FromSeconds(2),
                FailureCallback = (logEvent, exception) =>
                    Console.WriteLine("Unable to submit event " + logEvent.MessageTemplate + (exception != null ? "; Exception: " + exception.Message : string.Empty)),
                EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                                   EmitEventFailureHandling.RaiseCallback
            });

        // Apply overrides from configuration if they exist
        ApplyLogLevelOverride(loggerConfig, "Microsoft", minimumLevelConfig["Microsoft"], globalLogLevel);
        ApplyLogLevelOverride(loggerConfig, "Microsoft.AspNetCore", minimumLevelConfig["Microsoft.AspNetCore"], globalLogLevel);
        ApplyLogLevelOverride(loggerConfig, "Microsoft.EntityFrameworkCore", minimumLevelConfig["Microsoft.EntityFrameworkCore"], globalLogLevel);

        Log.Logger = loggerConfig.CreateLogger();
        builder.Host.UseSerilog();
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
}