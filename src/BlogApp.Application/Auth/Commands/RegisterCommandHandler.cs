namespace BlogApp.Application.Auth.Commands;

public class RegisterCommandHandler(
    UserManager<ApplicationUser> userManager,
    IMessageService messageService,
    IPasswordService passwordService) : IRequestHandler<RegisterCommand, ApiResponse<string>>
{
    public async Task<ApiResponse<string>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var userExists = await userManager.FindByEmailAsync(request.Email);
        if (userExists != null)
            return ApiResponse<string>.Failure(messageService.GetMessage("UserAlreadyExists"));

        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return ApiResponse<string>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));

        // Track initial password
        var passwordHash = userManager.PasswordHasher.HashPassword(user, request.Password);
        await passwordService.TrackPasswordChangeAsync(user.Id, passwordHash);

        return ApiResponse<string>.Success(messageService.GetMessage("UserCreatedSuccessfully"));
    }
}