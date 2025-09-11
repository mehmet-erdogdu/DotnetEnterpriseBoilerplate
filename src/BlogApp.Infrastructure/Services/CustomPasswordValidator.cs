namespace BlogApp.Infrastructure.Services;

public class CustomPasswordValidator<TUser>(IPasswordHistoryRepository passwordHistoryRepository) : IPasswordValidator<TUser>
    where TUser : class
{
    public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string? password)
    {
        var userId = await manager.GetUserIdAsync(user);

        // Check if password was recently used
        var passwordHash = manager.PasswordHasher.HashPassword(user, password!);
        var isRecentlyUsed = await passwordHistoryRepository.IsPasswordRecentlyUsedAsync(userId, passwordHash);

        if (isRecentlyUsed)
            return IdentityResult.Failed(new IdentityError
            {
                Code = "PasswordRecentlyUsed",
                Description = "This password was recently used and cannot be reused."
            });

        return IdentityResult.Success;
    }
}