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
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information)
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
            })
            .CreateLogger();
        builder.Host.UseSerilog();
    }
}