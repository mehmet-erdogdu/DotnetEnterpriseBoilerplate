namespace BlogApp.Application.Auth.Commands;

public class ChangePasswordCommand : IRequest<ApiResponse<string>>
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator(IMessageService messages)
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage(messages.GetMessage("CurrentPasswordRequired"));

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage(messages.GetMessage("PasswordRequired"))
            .MinimumLength(12).WithMessage(messages.GetMessage("PasswordLength"))
            .MaximumLength(128).WithMessage(messages.GetMessage("PasswordLength"))
            .Matches("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^\\da-zA-Z]).{12,}$").WithMessage(messages.GetMessage("PasswordComplexity"));

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(messages.GetMessage("UserIdRequired"));
    }
}