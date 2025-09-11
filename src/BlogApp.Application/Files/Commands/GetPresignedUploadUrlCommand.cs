namespace BlogApp.Application.Files.Commands;

public class GetPresignedUploadUrlCommand : IRequest<ApiResponse<PresignedUploadResponseDto>>
{
    public PresignedUploadRequestDto Request { get; set; } = new();
    public string UserId { get; set; } = string.Empty;
}

public class GetPresignedUploadUrlCommandValidator : AbstractValidator<GetPresignedUploadUrlCommand>
{
    public GetPresignedUploadUrlCommandValidator(IMessageService messages)
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(messages.GetMessage("UserIdRequired"));

        RuleFor(x => x.Request.FileName)
            .NotEmpty().WithMessage(messages.GetMessage("FileNameRequired"))
            .MaximumLength(255).WithMessage(messages.GetMessage("FileNameMax"));

        RuleFor(x => x.Request.ContentType)
            .NotEmpty().WithMessage(messages.GetMessage("ContentTypeRequired"))
            .MaximumLength(255).WithMessage(messages.GetMessage("ContentTypeMax"));

        RuleFor(x => x.Request.FileSize)
            .GreaterThan(0).WithMessage(messages.GetMessage("FileSizeMin"))
            .LessThanOrEqualTo(1048576 * 50).WithMessage(messages.GetMessage("FileSizeMax"));

        RuleFor(x => x.Request.Description)
            .MaximumLength(1000).WithMessage(messages.GetMessage("DescriptionMax"))
            .When(x => x.Request.Description != null);
    }
}