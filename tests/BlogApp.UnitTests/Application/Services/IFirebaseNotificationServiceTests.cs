using BlogApp.Application.Services;
using BlogApp.Application.DTOs;
using Moq;
using Xunit;

namespace BlogApp.UnitTests.Application.Services;

public class IFirebaseNotificationServiceTests
{
    [Fact]
    public void IFirebaseNotificationService_Should_Have_Expected_Methods()
    {
        // Arrange
        var mockService = new Mock<IFirebaseNotificationService>();
        var testTopic = "test-topic";
        var testToken = "test-token";
        var testUserId = "test-user-id";
        
        var notificationDto = new FirebaseNotificationDto
        {
            Title = "Test Notification",
            Body = "This is a test notification",
            ImageUrl = "https://example.com/image.jpg",
            Data = new Dictionary<string, string> { { "key", "value" } },
            ClickAction = "OPEN_ACTIVITY",
            Sound = "default",
            Priority = true,
            TimeToLive = 3600
        };
        
        var requestDto = new FirebaseNotificationRequestDto
        {
            TokenIds = new List<string> { testToken },
            Notification = notificationDto,
            Topic = testTopic,
            Data = new Dictionary<string, string> { { "key", "value" } }
        };
        
        var responseDto = new FirebaseNotificationResponseDto
        {
            Success = true,
            Message = "Notification sent successfully",
            SuccessCount = 1,
            FailureCount = 0,
            FailedTokens = new List<string>()
        };
        
        var deviceTokenDto = new DeviceTokenDto
        {
            UserId = testUserId,
            Token = testToken,
            Platform = "android",
            CreatedAt = DateTime.UtcNow,
            LastUsedAt = DateTime.UtcNow
        };
        
        // Act & Assert
        // Verify that the interface has the expected methods by setting up mock expectations
        mockService.Setup(x => x.SendNotificationAsync(requestDto))
            .ReturnsAsync(responseDto);
        mockService.Setup(x => x.SendNotificationToTopicAsync(testTopic, notificationDto, It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(responseDto);
        mockService.Setup(x => x.SubscribeToTopicAsync(testToken, testTopic))
            .ReturnsAsync(true);
        mockService.Setup(x => x.UnsubscribeFromTopicAsync(testToken, testTopic))
            .ReturnsAsync(true);
        mockService.Setup(x => x.SaveDeviceTokenAsync(deviceTokenDto))
            .ReturnsAsync(true);
        mockService.Setup(x => x.RemoveDeviceTokenAsync(testUserId, testToken))
            .ReturnsAsync(true);
        mockService.Setup(x => x.GetUserDeviceTokensAsync(testUserId))
            .ReturnsAsync(new List<string> { testToken });
        mockService.Setup(x => x.IsTokenValidAsync(testToken))
            .ReturnsAsync(true);
            
        // This test ensures the interface contract is as expected
        // If the interface changes, this test will help identify the change
    }
}