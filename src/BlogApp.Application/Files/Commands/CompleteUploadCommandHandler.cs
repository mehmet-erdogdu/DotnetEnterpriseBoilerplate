namespace BlogApp.Application.Files.Commands;

public class CompleteUploadCommandHandler(
    IFileService fileService,
    IMessageService messageService) : IRequestHandler<CompleteUploadCommand, ApiResponse<FileUploadResponseDto>>
{
    public async Task<ApiResponse<FileUploadResponseDto>> Handle(CompleteUploadCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await fileService.CompleteUploadAsync(request.CompleteDto, request.UserId);
            return ApiResponse<FileUploadResponseDto>.Success(result);
        }
        catch (FileNotFoundException)
        {
            return ApiResponse<FileUploadResponseDto>.Failure(messageService.GetMessage("FileNotFound"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return ApiResponse<FileUploadResponseDto>.Failure(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ApiResponse<FileUploadResponseDto>.Failure(ex.Message);
        }
        catch (Exception)
        {
            return ApiResponse<FileUploadResponseDto>.Failure(messageService.GetMessage("UploadCompletionFailed"));
        }
    }
}