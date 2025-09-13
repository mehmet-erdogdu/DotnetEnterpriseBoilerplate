using BlogApp.Domain.Entities;
using BlogApp.Infrastructure.Data;
using BlogApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace BlogApp.UnitTests.Infrastructure.Repositories;

public class PasswordHistoryRepositoryTests : BaseInfrastructureTest
{
    private readonly PasswordHistoryRepository _passwordHistoryRepository;
    private new readonly ApplicationDbContext _context;

    public PasswordHistoryRepositoryTests()
    {
        _context = CreateDbContext();
        _passwordHistoryRepository = new PasswordHistoryRepository(_context);
    }

    [Fact]
    public async Task GetPasswordHistoryByUserIdAsync_WithExistingHistory_ShouldReturnOrderedHistory()
    {
        // Arrange
        var userId = "test-user-id";
        var passwordHistory = new List<PasswordHistory>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, PasswordHash = "hash1", ChangedAt = DateTime.UtcNow.AddMinutes(-10) },
            new() { Id = Guid.NewGuid(), UserId = userId, PasswordHash = "hash2", ChangedAt = DateTime.UtcNow.AddMinutes(-5) },
            new() { Id = Guid.NewGuid(), UserId = userId, PasswordHash = "hash3", ChangedAt = DateTime.UtcNow.AddMinutes(-15) }
        };

        _context.PasswordHistory.AddRange(passwordHistory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _passwordHistoryRepository.GetPasswordHistoryByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        
        var resultList = result.ToList();
        // Should be ordered by ChangedAt descending
        resultList[0].PasswordHash.Should().Be("hash2"); // Most recent
        resultList[1].PasswordHash.Should().Be("hash1");
        resultList[2].PasswordHash.Should().Be("hash3"); // Oldest
    }

    [Fact]
    public async Task GetPasswordHistoryByUserIdAsync_WithNoHistory_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = "test-user-id";

        // Act
        var result = await _passwordHistoryRepository.GetPasswordHistoryByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task IsPasswordRecentlyUsedAsync_WithRecentlyUsedPassword_ShouldReturnTrue()
    {
        // Arrange
        var userId = "test-user-id";
        var passwordHash = "recent-hash";
        
        // Set environment variable for test
        Environment.SetEnvironmentVariable("Security__PasswordHistoryCount", "3");
        
        var passwordHistory = new List<PasswordHistory>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, PasswordHash = "old-hash", ChangedAt = DateTime.UtcNow.AddDays(-1) },
            new() { Id = Guid.NewGuid(), UserId = userId, PasswordHash = passwordHash, ChangedAt = DateTime.UtcNow.AddMinutes(-30) },
            new() { Id = Guid.NewGuid(), UserId = userId, PasswordHash = "another-hash", ChangedAt = DateTime.UtcNow.AddMinutes(-10) }
        };

        _context.PasswordHistory.AddRange(passwordHistory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _passwordHistoryRepository.IsPasswordRecentlyUsedAsync(userId, passwordHash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsPasswordRecentlyUsedAsync_WithNotRecentlyUsedPassword_ShouldReturnFalse()
    {
        // Arrange
        var userId = "test-user-id";
        var passwordHash = "new-hash";
        var oldPasswordHash = "old-hash";
        
        // Set environment variable for test
        Environment.SetEnvironmentVariable("Security__PasswordHistoryCount", "2");
        
        var passwordHistory = new List<PasswordHistory>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, PasswordHash = oldPasswordHash, ChangedAt = DateTime.UtcNow.AddDays(-1) },
            new() { Id = Guid.NewGuid(), UserId = userId, PasswordHash = "another-old-hash", ChangedAt = DateTime.UtcNow.AddHours(-2) }
        };

        _context.PasswordHistory.AddRange(passwordHistory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _passwordHistoryRepository.IsPasswordRecentlyUsedAsync(userId, passwordHash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsPasswordRecentlyUsedAsync_WithEmptyHistory_ShouldReturnFalse()
    {
        // Arrange
        var userId = "test-user-id";
        var passwordHash = "new-hash";
        
        // Set environment variable for test
        Environment.SetEnvironmentVariable("Security__PasswordHistoryCount", "5");

        // Act
        var result = await _passwordHistoryRepository.IsPasswordRecentlyUsedAsync(userId, passwordHash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetPasswordHistoryCountAsync_WithExistingHistory_ShouldReturnCorrectCount()
    {
        // Arrange
        var userId = "test-user-id";
        var otherUserId = "other-user-id";
        
        var passwordHistory = new List<PasswordHistory>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, PasswordHash = "hash1", ChangedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), UserId = userId, PasswordHash = "hash2", ChangedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), UserId = otherUserId, PasswordHash = "hash3", ChangedAt = DateTime.UtcNow }
        };

        _context.PasswordHistory.AddRange(passwordHistory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _passwordHistoryRepository.GetPasswordHistoryCountAsync(userId);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task GetPasswordHistoryCountAsync_WithNoHistory_ShouldReturnZero()
    {
        // Arrange
        var userId = "test-user-id";

        // Act
        var result = await _passwordHistoryRepository.GetPasswordHistoryCountAsync(userId);

        // Assert
        result.Should().Be(0);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _context?.Dispose();
        }
        base.Dispose(disposing);
    }
}