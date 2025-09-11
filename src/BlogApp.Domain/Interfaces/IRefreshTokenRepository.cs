namespace BlogApp.Domain.Interfaces;

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(string userId);
    Task RevokeTokenAsync(string token, string revokedBy, string? reason = null);
    Task RevokeAllUserTokensAsync(string userId, string revokedBy, string? reason = null);
    Task CleanupExpiredTokensAsync();
}