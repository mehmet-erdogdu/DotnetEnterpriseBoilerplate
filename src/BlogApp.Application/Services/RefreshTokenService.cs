using System.Security.Cryptography;

namespace BlogApp.Application.Services;

public class RefreshTokenService(IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork, IConfiguration configuration)
    : IRefreshTokenService
{
    public async Task<string> GenerateRefreshTokenAsync(string userId)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var refreshToken = Convert.ToBase64String(randomBytes);

        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(
                int.Parse(configuration["JWT:RefreshTokenExpirationDays"] ?? "7"))
        };

        await refreshTokenRepository.AddAsync(refreshTokenEntity);
        await unitOfWork.SaveChangesAsync();

        return refreshToken;
    }

    public async Task<bool> ValidateRefreshTokenAsync(string refreshToken)
    {
        var tokenEntity = await refreshTokenRepository.GetByTokenAsync(refreshToken);
        return tokenEntity != null && !tokenEntity.IsRevoked && tokenEntity.ExpiresAt > DateTime.UtcNow;
    }

    public async Task<string?> GetUserIdFromRefreshTokenAsync(string refreshToken)
    {
        var tokenEntity = await refreshTokenRepository.GetByTokenAsync(refreshToken);
        return tokenEntity?.UserId;
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, string revokedBy, string? reason = null)
    {
        await refreshTokenRepository.RevokeTokenAsync(refreshToken, revokedBy, reason);
    }

    public async Task RevokeAllUserTokensAsync(string userId, string revokedBy, string? reason = null)
    {
        await refreshTokenRepository.RevokeAllUserTokensAsync(userId, revokedBy, reason);
    }

    public async Task CleanupExpiredTokensAsync()
    {
        await refreshTokenRepository.CleanupExpiredTokensAsync();
    }
}

public interface IRefreshTokenService
{
    Task<string> GenerateRefreshTokenAsync(string userId);
    Task<bool> ValidateRefreshTokenAsync(string refreshToken);
    Task<string?> GetUserIdFromRefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken, string revokedBy, string? reason = null);
    Task RevokeAllUserTokensAsync(string userId, string revokedBy, string? reason = null);
    Task CleanupExpiredTokensAsync();
}