using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using System.IO;

namespace BlogApp.Infrastructure.Services;

public class FirebaseNotificationService : IFirebaseNotificationService
{
    private const string DeviceTokenCacheKey = "device_token_{0}"; // {0} = userId
    private const string TopicSubscriptionCacheKey = "topic_subscription_{0}"; // {0} = topic
    private readonly ICacheService _cacheService;
    private readonly FirebaseMessaging _firebaseMessaging;
    private readonly ILogger<FirebaseNotificationService> _logger;

    public FirebaseNotificationService(
        ILogger<FirebaseNotificationService> logger,
        ICacheService cacheService,
        IConfiguration configuration)
    {
        _logger = logger;
        _cacheService = cacheService;

        // Firebase Admin SDK initialization
        if (FirebaseApp.DefaultInstance == null)
            try
            {
                var projectId = configuration["Firebase:ProjectId"];
                var serviceAccountKeyPath = configuration["Firebase:ServiceAccountKeyPath"];

                if (!string.IsNullOrEmpty(serviceAccountKeyPath))
                {
                    var credential = GoogleCredential.FromServiceAccountCredential(ServiceAccountCredential.FromServiceAccountData(File.OpenRead(serviceAccountKeyPath)));
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = credential,
                        ProjectId = projectId
                    });
                    _logger.LogInformation("Firebase Admin SDK initialized successfully");
                }
                else
                {
                    _logger.LogWarning("Firebase ServiceAccountKeyPath is not configured");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Firebase Admin SDK");
            }

        _firebaseMessaging = FirebaseMessaging.DefaultInstance;
    }

    public async Task<FirebaseNotificationResponseDto> SendNotificationAsync(FirebaseNotificationRequestDto request)
    {
        try
        {
            if (request.TokenIds.Count == 0)
                return new FirebaseNotificationResponseDto
                {
                    Success = false,
                    Message = "No device tokens provided"
                };

            var response = new FirebaseNotificationResponseDto();
            var failedTokens = new List<string>();

            foreach (var token in request.TokenIds)
                try
                {
                    var message = CreateFirebaseMessage(token, request.Notification, request.Data);
                    var result = await _firebaseMessaging.SendAsync(message);

                    if (!string.IsNullOrEmpty(result))
                    {
                        response.SuccessCount++;
                        _logger.LogInformation("Notification sent successfully to token: {Token}", token);
                    }
                    else
                    {
                        response.FailureCount++;
                        failedTokens.Add(token);
                        _logger.LogWarning("Failed to send notification to token: {Token}", token);
                    }
                }
                catch (Exception ex)
                {
                    response.FailureCount++;
                    failedTokens.Add(token);
                    _logger.LogError(ex, "Error sending notification to token: {Token}", token);
                }

            response.Success = response.SuccessCount > 0;
            response.Message = $"Sent {response.SuccessCount} notifications, {response.FailureCount} failed";
            response.FailedTokens = failedTokens;

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendNotificationAsync");
            return new FirebaseNotificationResponseDto
            {
                Success = false,
                Message = "Internal server error"
            };
        }
    }

    public async Task<FirebaseNotificationResponseDto> SendNotificationToTopicAsync(string topic, FirebaseNotificationDto notification, Dictionary<string, string>? data = null)
    {
        try
        {
            var message = CreateFirebaseTopicMessage(topic, notification, data);
            var result = await _firebaseMessaging.SendAsync(message);

            if (!string.IsNullOrEmpty(result))
            {
                _logger.LogInformation("Topic notification sent successfully to topic: {Topic}", topic);
                return new FirebaseNotificationResponseDto
                {
                    Success = true,
                    Message = "Topic notification sent successfully",
                    SuccessCount = 1
                };
            }

            _logger.LogWarning("Failed to send topic notification to topic: {Topic}", topic);
            return new FirebaseNotificationResponseDto
            {
                Success = false,
                Message = "Failed to send topic notification"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending topic notification to topic: {Topic}", topic);
            return new FirebaseNotificationResponseDto
            {
                Success = false,
                Message = "Internal server error"
            };
        }
    }

    public async Task<bool> SubscribeToTopicAsync(string token, string topic)
    {
        try
        {
            var response = await _firebaseMessaging.SubscribeToTopicAsync(new[] { token }, topic);

            if (response.SuccessCount > 0)
            {
                _logger.LogInformation("Successfully subscribed token {Token} to topic {Topic}", token, topic);

                // Cache topic subscription
                var cacheKey = string.Format(TopicSubscriptionCacheKey, topic);
                await _cacheService.SetAsync($"{cacheKey}:{token}", true, TimeSpan.FromDays(30));

                return true;
            }

            _logger.LogWarning("Failed to subscribe token {Token} to topic {Topic}", token, topic);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing token {Token} to topic {Topic}", token, topic);
            return false;
        }
    }

    public async Task<bool> UnsubscribeFromTopicAsync(string token, string topic)
    {
        try
        {
            var response = await _firebaseMessaging.UnsubscribeFromTopicAsync(new[] { token }, topic);

            if (response.SuccessCount > 0)
            {
                _logger.LogInformation("Successfully unsubscribed token {Token} from topic {Topic}", token, topic);

                // Remove from cache
                var cacheKey = string.Format(TopicSubscriptionCacheKey, topic);
                await _cacheService.RemoveAsync($"{cacheKey}:{token}");

                return true;
            }

            _logger.LogWarning("Failed to unsubscribe token {Token} from topic {Topic}", token, topic);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing token {Token} from topic {Topic}", token, topic);
            return false;
        }
    }

    public async Task<bool> SaveDeviceTokenAsync(DeviceTokenDto deviceToken)
    {
        try
        {
            var cacheKey = string.Format(DeviceTokenCacheKey, deviceToken.UserId);
            var existingTokens = await GetUserDeviceTokensAsync(deviceToken.UserId);

            if (!existingTokens.Contains(deviceToken.Token)) existingTokens.Add(deviceToken.Token);

            var tokenData = JsonSerializer.Serialize(deviceToken);
            await _cacheService.SetAsync($"{cacheKey}:{deviceToken.Token}", tokenData, TimeSpan.FromDays(365));

            _logger.LogInformation("Device token saved for user {UserId}", deviceToken.UserId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving device token for user {UserId}", deviceToken.UserId);
            return false;
        }
    }

    public async Task<bool> RemoveDeviceTokenAsync(string userId, string token)
    {
        try
        {
            var cacheKey = string.Format(DeviceTokenCacheKey, userId);
            await _cacheService.RemoveAsync($"{cacheKey}:{token}");

            _logger.LogInformation("Device token removed for user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing device token for user {UserId}", userId);
            return false;
        }
    }

    public async Task<List<string>> GetUserDeviceTokensAsync(string userId)
    {
        try
        {
            var cacheKey = string.Format(DeviceTokenCacheKey, userId);
            var tokens = new List<string>();

            // This is a simplified implementation - in production you might want to use a proper database
            // or implement a more sophisticated caching strategy
            var cachedTokens = await _cacheService.GetAsync<List<string>>(cacheKey);
            if (cachedTokens != null) tokens.AddRange(cachedTokens);

            return tokens;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting device tokens for user {UserId}", userId);
            return new List<string>();
        }
    }

    public async Task<bool> IsTokenValidAsync(string token)
    {
        try
        {
            // Send a test message to validate token
            var testMessage = new Message
            {
                Token = token,
                Data = new Dictionary<string, string>
                {
                    { "test", "validation" }
                }
            };

            var result = await _firebaseMessaging.SendAsync(testMessage);
            return !string.IsNullOrEmpty(result);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Token validation failed for token: {Token}", token);
            return false;
        }
    }

    private static Message CreateFirebaseMessage(string token, FirebaseNotificationDto notification, Dictionary<string, string>? data)
    {
        var message = new Message
        {
            Token = token,
            Notification = new Notification
            {
                Title = notification.Title,
                Body = notification.Body,
                ImageUrl = notification.ImageUrl
            },
            Data = data ?? new Dictionary<string, string>(),
            Android = new AndroidConfig
            {
                Priority = notification.Priority ? Priority.High : Priority.Normal,
                Notification = new AndroidNotification
                {
                    ClickAction = notification.ClickAction,
                    Sound = notification.Sound
                }
            },
            Apns = new ApnsConfig
            {
                Aps = new Aps
                {
                    Sound = notification.Sound,
                    Badge = 1
                }
            },
            Webpush = new WebpushConfig
            {
                Notification = new WebpushNotification
                {
                    Icon = notification.ImageUrl
                }
            }
        };

        // Note: TTL is not directly supported in Firebase Admin SDK Message
        // You can implement custom logic or use Android-specific TTL if needed

        return message;
    }

    private static Message CreateFirebaseTopicMessage(string topic, FirebaseNotificationDto notification, Dictionary<string, string>? data)
    {
        var message = new Message
        {
            Topic = topic,
            Notification = new Notification
            {
                Title = notification.Title,
                Body = notification.Body,
                ImageUrl = notification.ImageUrl
            },
            Data = data ?? new Dictionary<string, string>(),
            Android = new AndroidConfig
            {
                Priority = notification.Priority ? Priority.High : Priority.Normal,
                Notification = new AndroidNotification
                {
                    ClickAction = notification.ClickAction,
                    Sound = notification.Sound
                }
            },
            Apns = new ApnsConfig
            {
                Aps = new Aps
                {
                    Sound = notification.Sound,
                    Badge = 1
                }
            },
            Webpush = new WebpushConfig
            {
                Notification = new WebpushNotification
                {
                    Icon = notification.ImageUrl
                }
            }
        };

        // Note: TTL is not directly supported in Firebase Admin SDK Message
        // You can implement custom logic or use Android-specific TTL if needed

        return message;
    }
}