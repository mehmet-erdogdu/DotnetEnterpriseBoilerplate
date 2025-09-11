namespace BlogApp.Application.Files.Commands;

public class GetPresignedUploadUrlCommandHandler(
    IFileService fileService,
    IMessageService messageService) : IRequestHandler<GetPresignedUploadUrlCommand, ApiResponse<PresignedUploadResponseDto>>
{
    public async Task<ApiResponse<PresignedUploadResponseDto>> Handle(GetPresignedUploadUrlCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await fileService.GetPresignedUploadUrlAsync(request.Request, request.UserId);
            return ApiResponse<PresignedUploadResponseDto>.Success(result);
        }
        catch (InvalidOperationException ex)
        {
            return ApiResponse<PresignedUploadResponseDto>.Failure(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return ApiResponse<PresignedUploadResponseDto>.Failure(ex.Message);
        }
        catch (Exception)
        {
            return ApiResponse<PresignedUploadResponseDto>.Failure(messageService.GetMessage("PresignedUrlGenerationFailed"));
        }
    }
}