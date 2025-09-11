namespace BlogApp.Application.Files.Queries;

public class GetFileQueryHandler(
    IFileService fileService,
    IMessageService messageService) : IRequestHandler<GetFileQuery, ApiResponse<FileDto>>
{
    public async Task<ApiResponse<FileDto>> Handle(GetFileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var file = await fileService.GetFileAsync(request.FileId);
            if (file == null)
                return ApiResponse<FileDto>.Failure(messageService.GetMessage("FileNotFound"));

            return ApiResponse<FileDto>.Success(file);
        }
        catch (Exception)
        {
            return ApiResponse<FileDto>.Failure(messageService.GetMessage("FileListFailed"));
        }
    }
}