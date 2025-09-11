namespace BlogApp.API.Extensions;

/// <summary>
///     Extension methods for ControllerBase to provide common functionality across all controllers
/// </summary>
/// <example>
///     Usage examples:
///     Option 1 - Direct validation check:
///     if (!ModelState.IsValid)
///     return this.CreateValidationErrorResponse&lt;string&gt;(ModelState);
///     Option 2 - Try pattern:
///     if (this.TryGetValidationErrorResponse&lt;string&gt;(ModelState, out var validationResponse))
///     return validationResponse;
/// </example>
public static class ControllerExtensions
{
    /// <summary>
    ///     Creates a standardized ApiResponse with validation errors from ModelState
    ///     Returns HTTP 200 with validation errors in the error field instead of HTTP 400
    /// </summary>
    /// <typeparam name="T">The type of data the ApiResponse should contain</typeparam>
    /// <param name="controller">The controller instance</param>
    /// <param name="modelState">The ModelState containing validation errors</param>
    /// <returns>ApiResponse with validation errors in standardized format</returns>
    public static ApiResponse<T> CreateValidationErrorResponse<T>(this ControllerBase controller, ModelStateDictionary modelState)
    {
        var validationErrors = modelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? []
            );

        return new ApiResponse<T>
        {
            IsSuccess = false,
            Error = new ErrorResponse("Validation failed", "VALIDATION_ERROR", validationErrors)
        };
    }

    /// <summary>
    ///     Checks if ModelState is invalid and returns a validation error response if so
    /// </summary>
    /// <typeparam name="T">The type of data the ApiResponse should contain</typeparam>
    /// <param name="controller">The controller instance</param>
    /// <param name="modelState">The ModelState to validate</param>
    /// <param name="validationResponse">The validation error response if ModelState is invalid</param>
    /// <returns>True if ModelState is invalid, false otherwise</returns>
    public static bool TryGetValidationErrorResponse<T>(this ControllerBase controller, ModelStateDictionary modelState, out ApiResponse<T> validationResponse)
    {
        if (!modelState.IsValid)
        {
            validationResponse = controller.CreateValidationErrorResponse<T>(modelState);
            return true;
        }

        validationResponse = null!;
        return false;
    }
}