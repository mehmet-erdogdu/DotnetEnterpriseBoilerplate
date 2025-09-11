namespace BlogApp.Application.DTOs;

public class RegisterDto
{
    public required string Email { get; set; }

    public required string Password { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }
}

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator(IMessageService messages)
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