namespace BlogApp.Application.Auth.Commands;

public class RegisterCommand : IRequest<ApiResponse<string>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator(IMessageService messages)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(messages.GetMessage("EmailRequired"))
            .MaximumLength(100).WithMessage(messages.GetMessage("EmailMax"))
            .EmailAddress().WithMessage(messages.GetMessage("EmailInvalid"));

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(messages.GetMessage("PasswordRequired"))
            .MinimumLength(12).WithMessage(messages.GetMessage("PasswordLength"))
            .MaximumLength(128).WithMessage(messages.GetMessage("PasswordLength"))
            .Matches("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^\\da-zA-Z]).{12,}$").WithMessage(messages.GetMessage("PasswordComplexity"));

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage(messages.GetMessage("FirstNameRequired"))
            .MinimumLength(2).WithMessage(messages.GetMessage("FirstNameLength"))
            .MaximumLength(50).WithMessage(messages.GetMessage("FirstNameLength"))
            .Matches("^[a-zA-ZğüşıöçĞÜŞİÖÇ\\s]+$").WithMessage(messages.GetMessage("FirstNameInvalid"));

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage(messages.GetMessage("LastNameRequired"))
            .MinimumLength(2).WithMessage(messages.GetMessage("LastNameLength"))
            .MaximumLength(50).WithMessage(messages.GetMessage("LastNameLength"))
            .Matches("^[a-zA-ZğüşıöçĞÜŞİÖÇ\\s]+$").WithMessage(messages.GetMessage("LastNameInvalid"));
    }
}