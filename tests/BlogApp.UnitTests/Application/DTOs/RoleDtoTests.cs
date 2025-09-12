namespace BlogApp.UnitTests.Application.DTOs;

public class RoleDtoTests
{
    [Fact]
    public void RoleDto_ShouldInitializeWithDefaultValues()
    {
        // Act
        var dto = new RoleDto();

        // Assert
        dto.Id.Should().Be(string.Empty);
        dto.Name.Should().Be(string.Empty);
        dto.NormalizedName.Should().Be(string.Empty);
        dto.CreatedAt.Should().BeNull();
        dto.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void RoleDto_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddDays(-30);
        var updatedAt = DateTime.UtcNow.AddDays(-1);

        // Act
        var dto = new RoleDto
        {
            Id = "role-123",
            Name = "Administrator",
            NormalizedName = "ADMINISTRATOR",
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        dto.Id.Should().Be("role-123");
        dto.Name.Should().Be("Administrator");
        dto.NormalizedName.Should().Be("ADMINISTRATOR");
        dto.CreatedAt.Should().Be(createdAt);
        dto.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void RoleDto_WithNullTimestamps_ShouldHandleGracefully()
    {
        // Act
        var dto = new RoleDto
        {
            Id = "role-456",
            Name = "User",
            NormalizedName = "USER",
            CreatedAt = null,
            UpdatedAt = null
        };

        // Assert
        dto.Id.Should().Be("role-456");
        dto.Name.Should().Be("User");
        dto.NormalizedName.Should().Be("USER");
        dto.CreatedAt.Should().BeNull();
        dto.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void RoleDto_WithEmptyStrings_ShouldHandleGracefully()
    {
        // Act
        var dto = new RoleDto
        {
            Id = "",
            Name = "",
            NormalizedName = "",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        dto.Id.Should().Be("");
        dto.Name.Should().Be("");
        dto.NormalizedName.Should().Be("");
        dto.CreatedAt.Should().NotBeNull();
        dto.UpdatedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData("admin", "ADMIN")]
    [InlineData("user", "USER")]
    [InlineData("moderator", "MODERATOR")]
    [InlineData("editor", "EDITOR")]
    [InlineData("viewer", "VIEWER")]
    public void RoleDto_WithVariousRoleNames_ShouldWorkCorrectly(string name, string normalizedName)
    {
        // Act
        var dto = new RoleDto
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            NormalizedName = normalizedName,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-5)
        };

        // Assert
        dto.Name.Should().Be(name);
        dto.NormalizedName.Should().Be(normalizedName);
    }

    [Fact]
    public void RoleDto_WithFutureTimestamps_ShouldWorkCorrectly()
    {
        // Arrange
        var futureCreatedAt = DateTime.UtcNow.AddDays(1);
        var futureUpdatedAt = DateTime.UtcNow.AddDays(2);

        // Act
        var dto = new RoleDto
        {
            Id = "future-role",
            Name = "FutureRole",
            NormalizedName = "FUTUREROLE",
            CreatedAt = futureCreatedAt,
            UpdatedAt = futureUpdatedAt
        };

        // Assert
        dto.CreatedAt.Should().Be(futureCreatedAt);
        dto.UpdatedAt.Should().Be(futureUpdatedAt);
    }

    [Fact]
    public void RoleDto_WithPastTimestamps_ShouldWorkCorrectly()
    {
        // Arrange
        var pastCreatedAt = DateTime.UtcNow.AddDays(-365);
        var pastUpdatedAt = DateTime.UtcNow.AddDays(-180);

        // Act
        var dto = new RoleDto
        {
            Id = "old-role",
            Name = "OldRole",
            NormalizedName = "OLDROLE",
            CreatedAt = pastCreatedAt,
            UpdatedAt = pastUpdatedAt
        };

        // Assert
        dto.CreatedAt.Should().Be(pastCreatedAt);
        dto.UpdatedAt.Should().Be(pastUpdatedAt);
    }

    [Fact]
    public void RoleDto_WithSameCreatedAndUpdatedAt_ShouldWorkCorrectly()
    {
        // Arrange
        var sameTimestamp = DateTime.UtcNow;

        // Act
        var dto = new RoleDto
        {
            Id = "same-time-role",
            Name = "SameTimeRole",
            NormalizedName = "SAMETIMEROLE",
            CreatedAt = sameTimestamp,
            UpdatedAt = sameTimestamp
        };

        // Assert
        dto.CreatedAt.Should().Be(sameTimestamp);
        dto.UpdatedAt.Should().Be(sameTimestamp);
        dto.CreatedAt.Should().Be(dto.UpdatedAt);
    }

    [Fact]
    public void RoleDto_WithUpdatedAtBeforeCreatedAt_ShouldWorkCorrectly()
    {
        // Arrange
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow.AddDays(-1); // Updated before created (edge case)

        // Act
        var dto = new RoleDto
        {
            Id = "weird-time-role",
            Name = "WeirdTimeRole",
            NormalizedName = "WEIRDTIMEROLE",
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        dto.CreatedAt.Should().Be(createdAt);
        dto.UpdatedAt.Should().Be(updatedAt);
        dto.UpdatedAt.Should().BeBefore(dto.CreatedAt!.Value);
    }

    [Fact]
    public void RoleDto_WithSpecialCharacters_ShouldWorkCorrectly()
    {
        // Act
        var dto = new RoleDto
        {
            Id = "role-with-special-chars-123",
            Name = "Role With Spaces & Symbols!",
            NormalizedName = "ROLE_WITH_SPACES_&_SYMBOLS!",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        dto.Id.Should().Be("role-with-special-chars-123");
        dto.Name.Should().Be("Role With Spaces & Symbols!");
        dto.NormalizedName.Should().Be("ROLE_WITH_SPACES_&_SYMBOLS!");
    }
}