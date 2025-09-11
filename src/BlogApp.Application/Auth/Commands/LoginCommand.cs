namespace BlogApp.Application.Auth.Commands;

public class LoginCommand : IRequest<ApiResponse<LoginResponseDto>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator(IMessageService messages)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(messages.GetMessage("EmailRequired"))
            .MaximumLength(100).WithMessage(messages.GetMessage("EmailMax"))
            .EmailAddress().WithMessage(messages.GetMessage("EmailInvalid"));

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(messages.GetMessage("PasswordRequired"))
            .MinimumLength(12).WithMessage(messages.GetMessage("PasswordLength"))
            .MaximumLength(128).WithMessage(messages.GetMessage("PasswordLength"));
    }
}