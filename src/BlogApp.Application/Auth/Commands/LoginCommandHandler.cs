namespace BlogApp.Application.Auth.Commands;

public class LoginCommandHandler(
    UserManager<ApplicationUser> userManager,
    IRefreshTokenService refreshTokenService,
    IMessageService messageService,
    IConfiguration configuration) : IRequestHandler<LoginCommand, ApiResponse<LoginResponseDto>>
{
    public async Task<ApiResponse<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return ApiResponse<LoginResponseDto>.Failure(messageService.GetMessage("InvalidCredentials"));

        var isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
            return ApiResponse<LoginResponseDto>.Failure(messageService.GetMessage("InvalidCredentials"));

        var accessToken = GenerateJwtToken(user);
        var refreshToken = await refreshTokenService.GenerateRefreshTokenAsync(user.Id);

        var response = new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(configuration["JWT:TokenExpirationMinutes"]!)),
            TokenType = "Bearer"
        };

        return ApiResponse<LoginResponseDto>.Success(response);
    }

    private string GenerateJwtToken(ApplicationUser user)
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