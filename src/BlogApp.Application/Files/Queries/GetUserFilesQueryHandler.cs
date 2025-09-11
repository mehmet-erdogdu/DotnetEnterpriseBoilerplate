namespace BlogApp.Application.Files.Queries;

public class GetUserFilesQueryHandler(
    IFileService fileService,
    IMessageService messageService) : IRequestHandler<GetUserFilesQuery, ApiResponse<IEnumerable<FileDto>>>
{
    public async Task<ApiResponse<IEnumerable<FileDto>>> Handle(GetUserFilesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var files = await fileService.GetUserFilesAsync(request.UserId);
            return ApiResponse<IEnumerable<FileDto>>.Success(files);
        }
        catch (Exception)
        {
            return ApiResponse<IEnumerable<FileDto>>.Failure(messageService.GetMessage("FileListFailed"));
        }
    }
}