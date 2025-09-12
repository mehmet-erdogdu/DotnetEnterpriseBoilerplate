using System.Net;

namespace BlogApp.UnitTests.Application.Services;

public class CurrentUserServiceTests : BaseTestClass
{
    [Fact]
    public void UserId_ReturnsCorrectValue_FromClaims()
    {
        // Arrange
        var userId = "test-user-id";
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, "test@example.com")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = principal };
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var service = new CurrentUserService(httpContextAccessor.Object);

        // Act
        var result = service.UserId;

        // Assert
        result.Should().Be(userId);
    }

    [Fact]
    public void UserId_ReturnsNull_WhenNoHttpContext()
    {
        // Arrange
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var service = new CurrentUserService(httpContextAccessor.Object);

        // Act
        var result = service.UserId;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void IpAddress_ReturnsRemoteIpAddress_WhenNoForwardedHeader()
    {
        // Arrange
        var ipAddress = IPAddress.Parse("192.168.1.100");
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = ipAddress;

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var service = new CurrentUserService(httpContextAccessor.Object);

        // Act
        var result = service.IpAddress;

        // Assert
        result.Should().Be("192.168.1.100");
    }

    [Fact]
    public void IpAddress_ReturnsForwardedForHeader_WhenPresent()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Forwarded-For"] = "203.0.113.195";

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var service = new CurrentUserService(httpContextAccessor.Object);

        // Act
        var result = service.IpAddress;

        // Assert
        result.Should().Be("203.0.113.195");
    }

    [Fact]
    public void IpAddress_ReturnsFirstForwardedIp_WhenMultiplePresent()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Forwarded-For"] = "203.0.113.195, 70.41.3.18, 150.172.238.178";

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var service = new CurrentUserService(httpContextAccessor.Object);

        // Act
        var result = service.IpAddress;

        // Assert
        result.Should().Be("203.0.113.195");
    }

    [Fact]
    public void IpAddress_ReturnsNull_WhenNoHttpContext()
    {
        // Arrange
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var service = new CurrentUserService(httpContextAccessor.Object);

        // Act
        var result = service.IpAddress;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void UserAgent_ReturnsHeaderValue_WhenPresent()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var service = new CurrentUserService(httpContextAccessor.Object);

        // Act
        var result = service.UserAgent;

        // Assert
        result.Should().Be("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
    }

    [Fact]
    public void UserAgent_ReturnsNull_WhenNoHttpContext()
    {
        // Arrange
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var service = new CurrentUserService(httpContextAccessor.Object);

        // Act
        var result = service.UserAgent;

        // Assert
        result.Should().BeNull();
    }
}