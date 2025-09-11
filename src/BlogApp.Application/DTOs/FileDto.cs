namespace BlogApp.Application.DTOs;

public record FileDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = default!;
    public string OriginalFileName { get; init; } = default!;
    public string ContentType { get; init; } = default!;
    public long FileSize { get; init; }
    public string FilePath { get; init; } = default!;
    public string? Description { get; init; }
    public string? UploadedById { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record UploadFileDto
{
    public string FileName { get; init; } = default!;
    public string ContentType { get; init; } = default!;
    public long FileSize { get; init; }
    public string? Description { get; init; }
}

public record FileUploadResponseDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = default!;
    public string OriginalFileName { get; init; } = default!;
    public string ContentType { get; init; } = default!;
    public long FileSize { get; init; }
    public string FilePath { get; init; } = default!;
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record PresignedUploadRequestDto
{
    public string FileName { get; init; } = default!;
    public string ContentType { get; init; } = default!;
    public long FileSize { get; init; }
    public string? Description { get; init; }
}

public record PresignedUploadResponseDto
{
    public Guid FileId { get; init; }
    public string UploadUrl { get; init; } = default!;
    public string FilePath { get; init; } = default!;
    public DateTime ExpiresAt { get; init; }
    public Dictionary<string, string> FormFields { get; init; } = new();
}

public record CompleteUploadDto
{
    public Guid FileId { get; init; }
    public long ActualFileSize { get; init; }
    public string? OriginalFileName { get; init; }
    public string? ContentType { get; init; }
}