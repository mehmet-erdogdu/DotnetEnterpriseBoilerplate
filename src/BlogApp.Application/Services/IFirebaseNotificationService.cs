namespace BlogApp.Application.Services;

public interface IFirebaseNotificationService
{
    Task<FirebaseNotificationResponseDto> SendNotificationAsync(FirebaseNotificationRequestDto request);
    Task<FirebaseNotificationResponseDto> SendNotificationToTopicAsync(string topic, FirebaseNotificationDto notification, Dictionary<string, string>? data = null);
    Task<bool> SubscribeToTopicAsync(string token, string topic);
    Task<bool> UnsubscribeFromTopicAsync(string token, string topic);
    Task<bool> SaveDeviceTokenAsync(DeviceTokenDto deviceToken);
    Task<bool> RemoveDeviceTokenAsync(string userId, string token);
    Task<List<string>> GetUserDeviceTokensAsync(string userId);
    Task<bool> IsTokenValidAsync(string token);
}