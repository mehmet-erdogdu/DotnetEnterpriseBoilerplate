namespace BlogApp.Application.Auth.Commands;

public class RefreshTokenCommandHandler(
    IRefreshTokenService refreshTokenService,
    UserManager<ApplicationUser> userManager,
    IMessageService messageService,
    IConfiguration configuration) : IRequestHandler<RefreshTokenCommand, ApiResponse<LoginResponseDto>>
{
    public async Task<ApiResponse<LoginResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        if (!await refreshTokenService.ValidateRefreshTokenAsync(request.RefreshToken))
            return ApiResponse<LoginResponseDto>.Failure(messageService.GetMessage("InvalidOrExpiredRefreshToken"));

        var userId = await refreshTokenService.GetUserIdFromRefreshTokenAsync(request.RefreshToken);
        if (userId == null)
            return ApiResponse<LoginResponseDto>.Failure(messageService.GetMessage("InvalidRefreshToken"));

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return ApiResponse<LoginResponseDto>.Failure(messageService.GetMessage("UserNotFound"));

        // Revoke the old refresh token
        await refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken, userId, "Refreshed");

        // Generate new tokens
        var accessToken = await GenerateJwtToken(user);
        var newRefreshToken = await refreshTokenService.GenerateRefreshTokenAsync(user.Id);

        var response = new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(configuration["JWT:TokenExpirationMinutes"]!)),
            TokenType = "Bearer"
        };

        return ApiResponse<LoginResponseDto>.Success(response);
    }

    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var jwtSecret = configuration["JWT:Secret"]!;
        var validIssuer = configuration["JWT:ValidIssuer"]!;
        var validAudience = configuration["JWT:ValidAudience"]!;
        var tokenExpirationMinutes = int.Parse(configuration["JWT:TokenExpirationMinutes"]!);

        var authClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add role claims
        var userRoles = await userManager.GetRolesAsync(user);
        foreach (var role in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Add role-specific claims
        foreach (var role in userRoles)
        {
            var roleEntity = await userManager.FindByNameAsync(role);
            if (roleEntity != null)
            {
                var roleClaims = await userManager.GetClaimsAsync(roleEntity);
                authClaims.AddRange(roleClaims);
            }
        }

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

        var token = new JwtSecurityToken(
            validIssuer,
            validAudience,
            expires: DateTime.UtcNow.AddMinutes(tokenExpirationMinutes),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}