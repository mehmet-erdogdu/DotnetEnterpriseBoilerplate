namespace BlogApp.Application.Todos.Queries;

public class GetTodosCountQueryHandler(ITodoRepository todoRepository) : IRequestHandler<GetTodosCountQuery, int>
{
    public async Task<int> Handle(GetTodosCountQuery request, CancellationToken cancellationToken)
    {
        return await todoRepository.GetCountByUserIdAsync(request.UserId);
    }
}