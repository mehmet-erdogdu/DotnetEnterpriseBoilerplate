using BlogApp.Domain.Entities;
using BlogApp.Infrastructure.Data;
using BlogApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace BlogApp.UnitTests.Infrastructure.Repositories;

public class RefreshTokenRepositoryTests : BaseInfrastructureTest
{
    private readonly RefreshTokenRepository _refreshTokenRepository;
    private new readonly ApplicationDbContext _context;

    public RefreshTokenRepositoryTests()
    {
        _context = CreateDbContext();
        _refreshTokenRepository = new RefreshTokenRepository(_context);
    }

    [Fact]
    public async Task GetByUserIdAsync_WithExistingTokens_ShouldReturnUserTokens()
    {
        // Arrange
        var userId = "test-user-id";
        var otherUserId = "other-user-id";
        
        var refreshTokens = new List<RefreshToken>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, Token = "token1", ExpiresAt = DateTime.UtcNow.AddDays(1) },
            new() { Id = Guid.NewGuid(), UserId = userId, Token = "token2", ExpiresAt = DateTime.UtcNow.AddDays(2) },
            new() { Id = Guid.NewGuid(), UserId = otherUserId, Token = "token3", ExpiresAt = DateTime.UtcNow.AddDays(1) }
        };

        _context.RefreshToken.AddRange(refreshTokens);
        await _context.SaveChangesAsync();

        // Act
        var result = await _refreshTokenRepository.GetActiveTokensByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.All(rt => rt.UserId == userId).Should().BeTrue();
    }

    [Fact]
    public async Task GetByUserIdAsync_WithNoTokens_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = "test-user-id";

        // Act
        var result = await _refreshTokenRepository.GetActiveTokensByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByTokenAsync_WithExistingToken_ShouldReturnToken()
    {
        // Arrange
        var tokenValue = "test-token";
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = "test-user-id",
            Token = tokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        _context.RefreshToken.Add(refreshToken);
        await _context.SaveChangesAsync();

        // Act
        var result = await _refreshTokenRepository.GetByTokenAsync(tokenValue);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().Be(tokenValue);
        result.UserId.Should().Be("test-user-id");
    }

    [Fact]
    public async Task GetByTokenAsync_WithNonExistingToken_ShouldReturnNull()
    {
        // Arrange
        var tokenValue = "non-existing-token";

        // Act
        var result = await _refreshTokenRepository.GetByTokenAsync(tokenValue);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RevokeAllUserTokensAsync_WithExistingTokens_ShouldRevokeAllUserTokens()
    {
        // Arrange
        var userId = "test-user-id";
        var otherUserId = "other-user-id";
        var revokedReason = "User logged out";
        var revokedBy = "system";
        
        var refreshTokens = new List<RefreshToken>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, Token = "token1", ExpiresAt = DateTime.UtcNow.AddDays(1), IsRevoked = false },
            new() { Id = Guid.NewGuid(), UserId = userId, Token = "token2", ExpiresAt = DateTime.UtcNow.AddDays(2), IsRevoked = false },
            new() { Id = Guid.NewGuid(), UserId = otherUserId, Token = "token3", ExpiresAt = DateTime.UtcNow.AddDays(1), IsRevoked = false }
        };

        _context.RefreshToken.AddRange(refreshTokens);
        await _context.SaveChangesAsync();

        // Act
        await _refreshTokenRepository.RevokeAllUserTokensAsync(userId, revokedBy, revokedReason);

        // Assert
        var userTokens = await _context.RefreshToken.Where(rt => rt.UserId == userId).ToListAsync();
        var otherUserTokens = await _context.RefreshToken.Where(rt => rt.UserId == otherUserId).ToListAsync();
        
        userTokens.Should().HaveCount(2);
        userTokens.All(rt => rt.IsRevoked).Should().BeTrue();
        userTokens.All(rt => rt.RevokedReason == revokedReason).Should().BeTrue();
        userTokens.All(rt => rt.RevokedBy == revokedBy).Should().BeTrue();
        
        // Other user's tokens should remain unchanged
        otherUserTokens.Should().HaveCount(1);
        otherUserTokens[0].IsRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task RevokeAllUserTokensAsync_WithNoTokens_ShouldNotThrowException()
    {
        // Arrange
        var userId = "test-user-id";
        var revokedReason = "User logged out";
        var revokedBy = "system";

        // Act
        Func<Task> act = async () => await _refreshTokenRepository.RevokeAllUserTokensAsync(userId, revokedBy, revokedReason);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RevokeTokenAsync_WithExistingToken_ShouldRevokeToken()
    {
        // Arrange
        var tokenValue = "test-token";
        var revokedReason = "Token compromised";
        var revokedBy = "user";
        
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = "test-user-id",
            Token = tokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false
        };

        _context.RefreshToken.Add(refreshToken);
        await _context.SaveChangesAsync();

        // Act
        await _refreshTokenRepository.RevokeTokenAsync(tokenValue, revokedBy, revokedReason);

        // Assert
        var result = await _context.RefreshToken.FirstOrDefaultAsync(rt => rt.Token == tokenValue);
        result.Should().NotBeNull();
        result!.IsRevoked.Should().BeTrue();
        result.RevokedReason.Should().Be(revokedReason);
        result.RevokedBy.Should().Be(revokedBy);
        result.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RevokeTokenAsync_WithNonExistingToken_ShouldNotThrowException()
    {
        // Arrange
        var tokenValue = "non-existing-token";
        var revokedReason = "Token compromised";
        var revokedBy = "user";

        // Act
        Func<Task> act = async () => await _refreshTokenRepository.RevokeTokenAsync(tokenValue, revokedBy, revokedReason);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RemoveExpiredTokensAsync_WithExpiredTokens_ShouldRemoveExpiredTokens()
    {
        // Arrange
        var now = DateTime.UtcNow;
        
        var refreshTokens = new List<RefreshToken>
        {
            new() { Id = Guid.NewGuid(), UserId = "user1", Token = "token1", ExpiresAt = now.AddDays(-31) }, // Expired more than 30 days ago
            new() { Id = Guid.NewGuid(), UserId = "user2", Token = "token2", ExpiresAt = now.AddDays(1) },   // Valid
            new() { Id = Guid.NewGuid(), UserId = "user3", Token = "token3", ExpiresAt = now.AddDays(-32) }  // Expired more than 30 days ago
        };

        _context.RefreshToken.AddRange(refreshTokens);
        await _context.SaveChangesAsync();

        // Act
        await _refreshTokenRepository.CleanupExpiredTokensAsync();

        // Assert
        var remainingTokens = await _context.RefreshToken.ToListAsync();
        remainingTokens.Should().HaveCount(1);
        remainingTokens[0].Token.Should().Be("token2"); // Only valid token should remain
    }

    [Fact]
    public async Task RemoveExpiredTokensAsync_WithNoExpiredTokens_ShouldNotRemoveAnyTokens()
    {
        // Arrange
        var now = DateTime.UtcNow;
        
        var refreshTokens = new List<RefreshToken>
        {
            new() { Id = Guid.NewGuid(), UserId = "user1", Token = "token1", ExpiresAt = now.AddDays(1) },
            new() { Id = Guid.NewGuid(), UserId = "user2", Token = "token2", ExpiresAt = now.AddDays(2) }
        };

        _context.RefreshToken.AddRange(refreshTokens);
        await _context.SaveChangesAsync();

        // Act
        await _refreshTokenRepository.CleanupExpiredTokensAsync();

        // Assert
        var remainingTokens = await _context.RefreshToken.ToListAsync();
        remainingTokens.Should().HaveCount(2);
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