namespace BlogApp.Application.DTOs;

public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;

    public string ConfirmNewPassword { get; set; } = string.Empty;
}

public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordDtoValidator(IMessageService messages)
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage(messages.GetMessage("CurrentPasswordRequired"));

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage(messages.GetMessage("PasswordRequired"))
            .MinimumLength(12).WithMessage(messages.GetMessage("PasswordLength"))
            .MaximumLength(128).WithMessage(messages.GetMessage("PasswordLength"))
            .Matches("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^\\da-zA-Z]).{12,}$").WithMessage(messages.GetMessage("PasswordComplexity"));

        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword).WithMessage(messages.GetMessage("PasswordsDoNotMatch"));
    }
}