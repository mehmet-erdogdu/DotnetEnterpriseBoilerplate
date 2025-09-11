namespace BlogApp.Application.Files.Commands;

public class CompleteUploadCommand : IRequest<ApiResponse<FileUploadResponseDto>>
{
    public CompleteUploadDto CompleteDto { get; set; } = new();
    public string UserId { get; set; } = string.Empty;
}

public class CompleteUploadCommandValidator : AbstractValidator<CompleteUploadCommand>
{
    public CompleteUploadCommandValidator(IMessageService messages)
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(messages.GetMessage("UserIdRequired"));

        RuleFor(x => x.CompleteDto.FileId)
            .NotEmpty().WithMessage(messages.GetMessage("FileIdRequired"));

        RuleFor(x => x.CompleteDto.ActualFileSize)
            .GreaterThan(0).WithMessage(messages.GetMessage("FileSizeMin"))
            .LessThanOrEqualTo(1048576 * 50).WithMessage(messages.GetMessage("FileSizeMax"));

        RuleFor(x => x.CompleteDto.OriginalFileName)
            .MaximumLength(255).WithMessage(messages.GetMessage("FileNameMax"))
            .When(x => x.CompleteDto.OriginalFileName != null);

        RuleFor(x => x.CompleteDto.ContentType)
            .MaximumLength(255).WithMessage(messages.GetMessage("ContentTypeMax"))
            .When(x => x.CompleteDto.ContentType != null);
    }
}