namespace BlogApp.Application.Files.Queries;

public class GetFilesCountQueryHandler(IFileRepository fileRepository) : IRequestHandler<GetFilesCountQuery, int>
{
    public async Task<int> Handle(GetFilesCountQuery request, CancellationToken cancellationToken)
    {
        return await fileRepository.GetCountByUserIdAsync(request.UserId);
    }
}