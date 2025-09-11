namespace BlogApp.Application.Files.Queries;

public class DownloadFileQueryHandler(
    IFileService fileService,
    IFileRepository fileRepository,
    IMessageService messageService) : IRequestHandler<DownloadFileQuery, DownloadFileResult>
{
    public async Task<DownloadFileResult> Handle(DownloadFileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // First get the file metadata to check permissions and get file info
            var file = await fileRepository.GetByIdAsync(request.FileId);
            if (file == null)
                return new DownloadFileResult
                {
                    Success = false,
                    ErrorMessage = messageService.GetMessage("FileNotFound")
                };

            // Check if user has permission to download this file
            // For now, only the owner can download (you can modify this logic as needed)
            if (file.UploadedById != request.UserId)
                return new DownloadFileResult
                {
                    Success = false,
                    ErrorMessage = messageService.GetMessage("FileAccessDenied")
                };

            // Get the file stream from MinIO
            var fileStream = await fileService.DownloadFileAsync(request.FileId);

            return new DownloadFileResult
            {
                Success = true,
                FileStream = fileStream,
                FileName = file.OriginalFileName,
                ContentType = file.ContentType,
                FileSize = file.FileSize
            };
        }
        catch (FileNotFoundException)
        {
            return new DownloadFileResult
            {
                Success = false,
                ErrorMessage = messageService.GetMessage("FileNotFound")
            };
        }
        catch (Exception)
        {
            return new DownloadFileResult
            {
                Success = false,
                ErrorMessage = messageService.GetMessage("FileDownloadFailed")
            };
        }
    }
}