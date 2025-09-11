namespace BlogApp.Application.DTOs;

public class LoginDto
{
    public required string Email { get; set; }

    public required string Password { get; set; }
}

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator(IMessageService messages)
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