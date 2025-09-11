namespace BlogApp.Application.Auth.Commands;

public class RefreshTokenCommand : IRequest<ApiResponse<LoginResponseDto>>
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator(IMessageService messages)
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage(messages.GetMessage("RefreshTokenRequired"))
            .MaximumLength(512).WithMessage(messages.GetMessage("RefreshTokenMax"));
    }
}