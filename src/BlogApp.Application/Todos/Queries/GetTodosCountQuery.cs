namespace BlogApp.Application.Todos.Queries;

public record GetTodosCountQuery(string UserId) : IRequest<int>;