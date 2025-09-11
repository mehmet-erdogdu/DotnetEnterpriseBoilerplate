namespace BlogApp.UnitTests.Domain.Entities;

public class PasswordHistoryTests : BaseTestClass
{
    [Fact]
    public void PasswordHistory_Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = "test-user-id";
        var passwordHash = "hashed-password";
        var changedAt = DateTime.UtcNow;
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow.AddMinutes(5);

        // Act
        var passwordHistory = new PasswordHistory
        {
            Id = id,
            UserId = userId,
            PasswordHash = passwordHash,
            ChangedAt = changedAt,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        passwordHistory.Id.Should().Be(id);
        passwordHistory.UserId.Should().Be(userId);
        passwordHistory.PasswordHash.Should().Be(passwordHash);
        passwordHistory.ChangedAt.Should().Be(changedAt);
        passwordHistory.CreatedAt.Should().Be(createdAt);
        passwordHistory.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void PasswordHistory_WithRequiredProperties_ShouldBeValid()
    {
        // Act
        var passwordHistory = new PasswordHistory
        {
            UserId = "test-user-id",
            PasswordHash = "hashed-password",
            ChangedAt = DateTime.UtcNow
        };

        // Assert
        passwordHistory.UserId.Should().Be("test-user-id");
        passwordHistory.PasswordHash.Should().Be("hashed-password");
        passwordHistory.ChangedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}