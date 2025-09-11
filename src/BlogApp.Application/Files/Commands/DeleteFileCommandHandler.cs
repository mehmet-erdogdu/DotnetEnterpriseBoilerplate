namespace BlogApp.Application.Files.Commands;

public class DeleteFileCommandHandler(
    IFileService fileService,
    IMessageService messageService) : IRequestHandler<DeleteFileCommand, ApiResponse<bool>>
{
    public async Task<ApiResponse<bool>> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await fileService.DeleteFileAsync(request.FileId, request.UserId);
            if (!result)
                return ApiResponse<bool>.Failure(messageService.GetMessage("FileOperationUnauthorized"));

            return ApiResponse<bool>.Success(true);
        }
        catch (Exception)
        {
            return ApiResponse<bool>.Failure(messageService.GetMessage("FileDeleteFailed"));
        }
    }
}