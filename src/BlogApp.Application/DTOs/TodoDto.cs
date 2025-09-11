namespace BlogApp.Application.DTOs;

public class TodoDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public required string UserId { get; set; }
    public string? UserName { get; set; }
}