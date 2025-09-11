namespace BlogApp.UnitTests.Domain.Entities;

public class RefreshTokenTests : BaseTestClass
{
    [Fact]
    public void RefreshToken_Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var id = Guid.NewGuid();
        var token = "test-refresh-token";
        var userId = "test-user-id";
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow.AddMinutes(5);

        // Act
        var refreshToken = new RefreshToken
        {
            Id = id,
            Token = token,
            UserId = userId,
            ExpiresAt = expiresAt,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        refreshToken.Id.Should().Be(id);
        refreshToken.Token.Should().Be(token);
        refreshToken.UserId.Should().Be(userId);
        refreshToken.ExpiresAt.Should().Be(expiresAt);
        refreshToken.CreatedAt.Should().Be(createdAt);
        refreshToken.UpdatedAt.Should().Be(updatedAt);
        refreshToken.IsRevoked.Should().BeFalse(); // Default value
        refreshToken.RevokedAt.Should().BeNull(); // Default value
        refreshToken.RevokedBy.Should().BeNull(); // Default value
        refreshToken.RevokedReason.Should().BeNull(); // Default value
    }

    [Fact]
    public void RefreshToken_WithRequiredProperties_ShouldBeValid()
    {
        // Act
        var refreshToken = new RefreshToken
        {
            Token = "test-refresh-token",
            UserId = "test-user-id",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        // Assert
        refreshToken.Token.Should().Be("test-refresh-token");
        refreshToken.UserId.Should().Be("test-user-id");
        refreshToken.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(1));
        refreshToken.IsRevoked.Should().BeFalse(); // Default value
        refreshToken.RevokedAt.Should().BeNull(); // Default value
        refreshToken.RevokedBy.Should().BeNull(); // Default value
        refreshToken.RevokedReason.Should().BeNull(); // Default value
    }

    [Fact]
    public void RefreshToken_WhenRevoked_ShouldSetRevokedProperties()
    {
        // Arrange
        var refreshToken = new RefreshToken
        {
            Token = "test-refresh-token",
            UserId = "test-user-id",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        var revokedAt = DateTime.UtcNow;
        var revokedBy = "test-admin";
        var revokedReason = "Testing";

        // Act
        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = revokedAt;
        refreshToken.RevokedBy = revokedBy;
        refreshToken.RevokedReason = revokedReason;

        // Assert
        refreshToken.IsRevoked.Should().BeTrue();
        refreshToken.RevokedAt.Should().Be(revokedAt);
        refreshToken.RevokedBy.Should().Be(revokedBy);
        refreshToken.RevokedReason.Should().Be(revokedReason);
    }

    [Fact]
    public void RefreshToken_WhenUnrevoked_ShouldClearRevokedProperties()
    {
        // Arrange
        var refreshToken = new RefreshToken
        {
            Token = "test-refresh-token",
            UserId = "test-user-id",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = true,
            RevokedAt = DateTime.UtcNow,
            RevokedBy = "test-admin",
            RevokedReason = "Testing"
        };

        // Act
        refreshToken.IsRevoked = false;
        refreshToken.RevokedAt = null;
        refreshToken.RevokedBy = null;
        refreshToken.RevokedReason = null;

        // Assert
        refreshToken.IsRevoked.Should().BeFalse();
        refreshToken.RevokedAt.Should().BeNull();
        refreshToken.RevokedBy.Should().BeNull();
        refreshToken.RevokedReason.Should().BeNull();
    }
}