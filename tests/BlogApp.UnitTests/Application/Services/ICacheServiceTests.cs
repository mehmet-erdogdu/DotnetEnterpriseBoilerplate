using BlogApp.Application.Services;
using Moq;
using Xunit;

namespace BlogApp.UnitTests.Application.Services;

public class ICacheServiceTests
{
    [Fact]
    public void ICacheService_Should_Have_Expected_Methods()
    {
        // Arrange
        var mockService = new Mock<ICacheService>();
        var testKey = "test-key";
        var testValue = "test-value";
        var testExpiration = TimeSpan.FromMinutes(5);
        
        // Act & Assert
        // Verify that the interface has the expected methods by setting up mock expectations
        mockService.Setup(x => x.GetAsync<string>(testKey))
            .ReturnsAsync(testValue);
        mockService.Setup(x => x.SetAsync(testKey, testValue, testExpiration))
            .Returns(Task.CompletedTask);
        mockService.Setup(x => x.RemoveAsync(testKey))
            .Returns(Task.CompletedTask);
        mockService.Setup(x => x.RemoveByPatternAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        mockService.Setup(x => x.ExistsAsync(testKey))
            .ReturnsAsync(true);
        mockService.Setup(x => x.IncrementAsync(testKey, 1))
            .ReturnsAsync(1L);
        mockService.Setup(x => x.GetTimeToLiveAsync(testKey))
            .ReturnsAsync(testExpiration);
            
        // This test ensures the interface contract is as expected
        // If the interface changes, this test will help identify the change
    }
}