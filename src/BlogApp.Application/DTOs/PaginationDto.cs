namespace BlogApp.Application.DTOs;

public class PaginationDto
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public string? Search { get; set; }
}