namespace BlogApp.Application.DTOs;

public class PostDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public required string AuthorId { get; set; }
    public string? AuthorName { get; set; }
}