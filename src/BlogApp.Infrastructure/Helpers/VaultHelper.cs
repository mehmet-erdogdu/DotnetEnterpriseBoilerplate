using System.Diagnostics.CodeAnalysis;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;

namespace BlogApp.Infrastructure.Helpers;

[ExcludeFromCodeCoverage]
public static class VaultHelper
{
    private const string WarningLevel = "Warning";

    public static async Task<Dictionary<string, string?>> LoadSecretsFromVaultAsync(string appName)
    {
        var vaultAddress = Environment.GetEnvironmentVariable(appName + "_VAULT_ADDR");
        var vaultToken = Environment.GetEnvironmentVariable(appName + "_VAULT_TOKEN");

        try
        {
            var authMethod = new TokenAuthMethodInfo(vaultToken);
            var vaultClientSettings = new VaultClientSettings(vaultAddress, authMethod) { Namespace = null };
            var vaultClient = new VaultClient(vaultClientSettings);

            // Read from KV v2 at mount "secret" and path appName
            var secret = await vaultClient.V1.Secrets.KeyValue.V2
                .ReadSecretAsync(appName, mountPoint: "secret");
            var data = secret?.Data?.Data ?? new Dictionary<string, object?>();

            Console.WriteLine($"Raw data from Vault: {data.Count} items");

            // Convert to flattened dictionary
            var flattened = FlattenVaultData(data);
            Console.WriteLine($"Flattened data: {flattened.Count} items");

            foreach (var kvp in flattened.Take(5))
                Console.WriteLine($"  {kvp.Key} = {kvp.Value}");

            if (flattened.Count == 0)
                throw new InvalidOperationException("Vault can not initialize.");

            return flattened;
        }
        catch
        {
            // Fallback: load from local VAULT.json for development/CI
            var vaultJsonPath = Path.Combine(AppContext.BaseDirectory, "VAULT.json");
            if (!File.Exists(vaultJsonPath))
            {
                // Try API project VAULT.json
                var altPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "BlogApp.API", "VAULT.json");
                vaultJsonPath = File.Exists(altPath) ? altPath : vaultJsonPath;
            }

            if (File.Exists(vaultJsonPath))
            {
                var json = await File.ReadAllTextAsync(vaultJsonPath);
                var jsonData = JsonSerializer.Deserialize<Dictionary<string, object>>(json)!;
                return FlattenJsonToDictionary(jsonData);
            }

            // Minimal defaults to keep unit tests running
            return GetDefaultSecrets();
        }
    }

    public static void ConfigureSecretsInConfiguration(this IConfigurationBuilder configurationBuilder, Dictionary<string, string?> secrets)
    {
        configurationBuilder.AddInMemoryCollection(secrets!);
    }

    private static Dictionary<string, string?> FlattenVaultData(IDictionary<string, object?> data, string prefix = "")
    {
        var result = new Dictionary<string, string?>();

        foreach (var kvp in data)
        {
            var key = BuildKey(prefix, kvp.Key);
            ProcessVaultValue(kvp.Value, key, result);
        }

        return result;
    }

    private static void ProcessVaultValue(object? value, string key, Dictionary<string, string?> result)
    {
        if (value == null)
        {
            result[key] = null;
            return;
        }

        switch (value)
        {
            case JsonElement jsonElement:
                ProcessJsonElement<Dictionary<string, object?>>(jsonElement, key, result, FlattenVaultData);
                break;
            case IDictionary<string, object?> nestedDict:
                ProcessNestedVaultDict(nestedDict, key, result);
                break;
            default:
                result[key] = value.ToString();
                break;
        }
    }

    private static void ProcessNestedVaultDict(IDictionary<string, object?> nestedDict, string key, Dictionary<string, string?> result)
    {
        var nestedResult = FlattenVaultData(nestedDict, key);
        foreach (var nested in nestedResult)
            result[nested.Key] = nested.Value;
    }

    private static Dictionary<string, string?> FlattenJsonToDictionary(Dictionary<string, object> jsonData, string prefix = "")
    {
        var result = new Dictionary<string, string?>();

        foreach (var kvp in jsonData)
        {
            var key = BuildKey(prefix, kvp.Key);
            ProcessJsonValue(kvp.Value, key, result);
        }

        return result;
    }

    private static void ProcessJsonValue(object value, string key, Dictionary<string, string?> result)
    {
        switch (value)
        {
            case JsonElement jsonElement:
                ProcessJsonElement<Dictionary<string, object>>(jsonElement, key, result, FlattenJsonToDictionary);
                break;
            case Dictionary<string, object> nestedDict:
                ProcessNestedJsonDict(nestedDict, key, result);
                break;
            default:
                result[key] = value?.ToString();
                break;
        }
    }

    private static void ProcessNestedJsonDict(Dictionary<string, object> nestedDict, string key, Dictionary<string, string?> result)
    {
        var nestedResult = FlattenJsonToDictionary(nestedDict, key);
        foreach (var nested in nestedResult)
            result[nested.Key] = nested.Value;
    }

    private static void ProcessJsonElement<T>(JsonElement jsonElement, string key, Dictionary<string, string?> result, Func<T, string, Dictionary<string, string?>> flattenFunc)
        where T : class
    {
        if (jsonElement.ValueKind == JsonValueKind.Object)
        {
            var nestedDict = JsonSerializer.Deserialize<T>(jsonElement.GetRawText())!;
            var nestedResult = flattenFunc(nestedDict, key);
            foreach (var nested in nestedResult)
                result[nested.Key] = nested.Value;
        }
        else
        {
            result[key] = jsonElement.ToString();
        }
    }

    private static string BuildKey(string prefix, string key)
    {
        return string.IsNullOrEmpty(prefix) ? key : $"{prefix}:{key}";
    }

    private static Dictionary<string, string?> GetDefaultSecrets()
    {
        return new Dictionary<string, string?>
        {
            ["DBConnection"] = string.Empty,
            ["AllowedHosts"] = "*",
            ["RedisConnection"] = "localhost:6379",
            ["ElasticUrl"] = "http://localhost:9200",
            ["APIUrl"] = "http://localhost",
            ["FrontendUrls"] = "http://localhost:3000,https://localhost:7266",
            ["KnownProxies"] = "127.0.0.1",
            ["TickerQBasicAuth:Username"] = "u",
            ["TickerQBasicAuth:Password"] = "p",
            ["JWT:RefreshTokenExpirationDays"] = "7",
            ["JWT:Secret"] = new('x', 64),
            ["JWT:TokenExpirationMinutes"] = "60",
            ["JWT:ValidAudience"] = "aud",
            ["JWT:ValidIssuer"] = "iss",
            ["S3:Url"] = "http://localhost:9000",
            ["S3:AccessKeyId"] = "ak",
            ["S3:SecretAccessKey"] = "sk",
            ["RabbitMQ:HostName"] = "localhost",
            ["RabbitMQ:Port"] = "5672",
            ["RabbitMQ:UserName"] = "guest",
            ["RabbitMQ:Password"] = "guest",
            ["RabbitMQ:VirtualHost"] = "/",
            ["RateLimiting:AuthLimit"] = "5",
            ["RateLimiting:AuthWindowMinutes"] = "1",
            ["RateLimiting:GlobalLimit"] = "50",
            ["RateLimiting:QueueLimit"] = "1",
            ["RateLimiting:WindowMinutes"] = "1",
            ["Security:LockoutDurationMinutes"] = "5",
            ["Security:MaxLoginAttempts"] = "5",
            ["Security:PasswordHistoryCount"] = "5",
            ["Security:RequireHttps"] = "false",
            ["Serilog:GlobalLogLevel"] = WarningLevel,
            ["Serilog:MinimumLevel:Default"] = WarningLevel,
            ["Serilog:MinimumLevel:Microsoft"] = WarningLevel,
            ["Serilog:MinimumLevel:Microsoft.AspNetCore"] = WarningLevel,
            ["Serilog:MinimumLevel:Microsoft.EntityFrameworkCore"] = WarningLevel,
            ["Firebase:ProjectId"] = "pid",
            ["Firebase:ServiceAccountKeyPath"] = "key.json"
        };
    }
}