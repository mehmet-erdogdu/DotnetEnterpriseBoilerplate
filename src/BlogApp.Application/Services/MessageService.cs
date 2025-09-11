using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace BlogApp.Application.Services;

public interface IMessageService
{
    string GetMessage(string key, params object[] args);
}

public class MessageService(IHttpContextAccessor httpContextAccessor) : IMessageService
{
    private readonly Dictionary<string, Dictionary<string, string>> _messageCache = new();

    public string GetMessage(string key, params object[] args)
    {
        var currentLanguage = GetCurrentLanguage();
        var messages = GetMessagesForLanguage(currentLanguage);

        if (!messages.TryGetValue(key, out var message))
            return key; // Return the key if message not found

        return args.Length > 0 ? string.Format(message, args) : message;
    }

    private string GetCurrentLanguage()
    {
        var acceptLanguage = httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"].ToString();
        if (string.IsNullOrEmpty(acceptLanguage))
            return "en-US"; // Default language

        // Get the first preferred language
        var preferredLanguage = acceptLanguage.Split(',')[0].Trim();

        // Check if we have a message file for this language, if not fallback to en-US
        var availableLanguages = new[] { "en-US", "tr-TR" };
        return availableLanguages.Contains(preferredLanguage) ? preferredLanguage : "en-US";
    }

    private Dictionary<string, string> GetMessagesForLanguage(string language)
    {
        if (_messageCache.TryGetValue(language, out var data))
            return data;

        var filePath = Path.Combine(AppContext.BaseDirectory, "Resources", $"MessageService.{language}.json");
        if (!File.Exists(filePath))
            filePath = Path.Combine(AppContext.BaseDirectory, "Resources", "MessageService.en-US.json");

        var jsonContent = File.ReadAllText(filePath);
        var messages = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);

        _messageCache[language] = messages ?? new Dictionary<string, string>();
        return _messageCache[language];
    }
}