namespace BlogApp.Application.DTOs;

public class FirebaseNotificationDto
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public Dictionary<string, string>? Data { get; set; }
    public string? ClickAction { get; set; }
    public string? Sound { get; set; } = "default";
    public bool Priority { get; set; } = true;
    public int? TimeToLive { get; set; }
}

public class FirebaseNotificationRequestDto
{
    public List<string> TokenIds { get; set; } = new();
    public FirebaseNotificationDto Notification { get; set; } = new();
    public string? Topic { get; set; }
    public Dictionary<string, string>? Data { get; set; }
}

public class FirebaseNotificationResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> FailedTokens { get; set; } = new();
}

public class DeviceTokenDto
{
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty; // "android", "ios", "web"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsedAt { get; set; }
}

public class TopicSubscriptionDto
{
    public string UserId { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
}

// Additional DTOs for controller requests
public class TopicNotificationRequestDto
{
    public string Topic { get; set; } = string.Empty;
    public FirebaseNotificationDto Notification { get; set; } = new();
    public Dictionary<string, string>? Data { get; set; }
}

public class TopicSubscriptionRequestDto
{
    public string Token { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
}

public class SaveDeviceTokenRequestDto
{
    public string Token { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty; // "android", "ios", "web"
}

public class ValidateTokenRequestDto
{
    public string Token { get; set; } = string.Empty;
}