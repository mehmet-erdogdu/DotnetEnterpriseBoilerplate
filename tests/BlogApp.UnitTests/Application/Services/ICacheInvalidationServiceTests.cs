using BlogApp.Application.Services;
using Moq;
using Xunit;

namespace BlogApp.UnitTests.Application.Services;

public class ICacheInvalidationServiceTests
{
    [Fact]
    public void ICacheInvalidationService_Should_Have_Expected_Methods()
    {
        // Arrange
        var mockService = new Mock<ICacheInvalidationService>();
        
        // Act & Assert
        // Verify that the interface has the expected methods by setting up mock expectations
        mockService.Setup(x => x.InvalidatePostCacheAsync(It.IsAny<Guid?>()))
            .Returns(Task.CompletedTask);
        mockService.Setup(x => x.InvalidateTodoCacheAsync(It.IsAny<Guid?>()))
            .Returns(Task.CompletedTask);
        mockService.Setup(x => x.InvalidateUserCacheAsync(It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        mockService.Setup(x => x.InvalidateAllCacheAsync())
            .Returns(Task.CompletedTask);
            
        // This test ensures the interface contract is as expected
        // If the interface changes, this test will help identify the change
    }
}