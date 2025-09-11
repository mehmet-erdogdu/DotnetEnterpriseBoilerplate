namespace BlogApp.Application.DTOs;

public record ValidationResult
{
    public bool IsValid { get; init; }
    public List<string> Errors { get; init; } = new();
    public List<string> Warnings { get; init; } = new();

    public static ValidationResult Success()
    {
        return new ValidationResult { IsValid = true };
    }

    public static ValidationResult Failure(params string[] errors)
    {
        return new ValidationResult
        {
            IsValid = false,
            Errors = errors.ToList()
        };
    }

    public static ValidationResult Failure(IEnumerable<string> errors)
    {
        return new ValidationResult
        {
            IsValid = false,
            Errors = errors.ToList()
        };
    }

    public ValidationResult AddError(string error)
    {
        return this with
        {
            IsValid = false,
            Errors = Errors.Append(error).ToList()
        };
    }

    public ValidationResult Combine(ValidationResult other)
    {
        return new ValidationResult
        {
            IsValid = IsValid && other.IsValid,
            Errors = Errors.Concat(other.Errors).ToList(),
            Warnings = Warnings.Concat(other.Warnings).ToList()
        };
    }
}