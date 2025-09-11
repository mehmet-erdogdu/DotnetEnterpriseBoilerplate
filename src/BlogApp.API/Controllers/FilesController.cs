namespace BlogApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController(
    IMediator mediator,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpPost("presigned-upload-url")]
    public async Task<ApiResponse<PresignedUploadResponseDto>> GetPresignedUploadUrl([FromBody] PresignedUploadRequestDto request)
    {
        if (!ModelState.IsValid)
            return this.CreateValidationErrorResponse<PresignedUploadResponseDto>(ModelState);

        var command = new GetPresignedUploadUrlCommand
        {
            Request = request,
            UserId = currentUserService.UserId
        };

        var result = await mediator.Send(command);
        await unitOfWork.SaveChangesAsync();
        return result;
    }

    [HttpPost("complete-upload")]
    public async Task<ApiResponse<FileUploadResponseDto>> CompleteUpload([FromBody] CompleteUploadDto completeDto)
    {
        if (!ModelState.IsValid)
            return this.CreateValidationErrorResponse<FileUploadResponseDto>(ModelState);

        var command = new CompleteUploadCommand
        {
            CompleteDto = completeDto,
            UserId = currentUserService.UserId
        };

        return await mediator.Send(command);
    }

    [HttpGet("{fileId}")]
    public async Task<ApiResponse<FileDto>> GetFile(Guid fileId)
    {
        var query = new GetFileQuery { FileId = fileId };
        return await mediator.Send(query);
    }

    [HttpGet("{fileId}/download")]
    public async Task<IActionResult> DownloadFile(Guid fileId)
    {
        var query = new DownloadFileQuery
        {
            FileId = fileId,
            UserId = currentUserService.UserId
        };

        var result = await mediator.Send(query);

        if (!result.Success)
            return NotFound(ApiResponse<object>.Failure(result.ErrorMessage ?? "File not found"));

        return File(result.FileStream, result.ContentType, result.FileName);
    }

    [HttpDelete("{fileId}")]
    public async Task<ApiResponse<bool>> DeleteFile(Guid fileId)
    {
        var command = new DeleteFileCommand
        {
            FileId = fileId,
            UserId = currentUserService.UserId
        };

        var result = await mediator.Send(command);
        await unitOfWork.SaveChangesAsync();
        return result;
    }

    [HttpGet("my-files")]
    public async Task<ApiResponse<IEnumerable<FileDto>>> GetMyFiles()
    {
        var query = new GetUserFilesQuery { UserId = currentUserService.UserId };
        return await mediator.Send(query);
    }
}