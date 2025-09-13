using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;

namespace BlogApp.UnitTests.API.Middleware;

public class SuccessContentTypeFilterTests : BaseTestClass
{
    [Fact]
    public async Task OnResultExecutionAsync_WithSuccessStatusCode_SetsJsonContentType()
    {
        // Arrange
        var filter = new SuccessContentTypeFilter();
        var httpContext = new DefaultHttpContext();
        httpContext.Response.StatusCode = 200;

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var resultContext = new ResultExecutingContext(
            actionContext,
            Array.Empty<IFilterMetadata>(),
            new ObjectResult(new { Message = "Success" }),
            null!);

        var nextCalled = false;
        ResultExecutionDelegate next = () =>
        {
            nextCalled = true;
            var executedContext = new ResultExecutedContext(
                resultContext,
                Array.Empty<IFilterMetadata>(),
                resultContext.Result,
                null!);
            return Task.FromResult(executedContext);
        };

        // Act
        await filter.OnResultExecutionAsync(resultContext, next);

        // Assert
        httpContext.Response.ContentType.Should().Be("application/json");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task OnResultExecutionAsync_WithFileResult_DoesNotChangeContentType()
    {
        // Arrange
        var filter = new SuccessContentTypeFilter();
        var httpContext = new DefaultHttpContext();
        httpContext.Response.StatusCode = 200;
        // Set the content type as FileResult would
        httpContext.Response.ContentType = "text/plain";

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var resultContext = new ResultExecutingContext(
            actionContext,
            Array.Empty<IFilterMetadata>(),
            new FileContentResult(Array.Empty<byte>(), "text/plain"),
            null!);

        var nextCalled = false;
        ResultExecutionDelegate next = () =>
        {
            nextCalled = true;
            var executedContext = new ResultExecutedContext(
                resultContext,
                Array.Empty<IFilterMetadata>(),
                resultContext.Result,
                null!);
            return Task.FromResult(executedContext);
        };

        // Act
        await filter.OnResultExecutionAsync(resultContext, next);

        // Assert
        // The filter should not change the content type that was set by FileResult
        httpContext.Response.ContentType.Should().Be("text/plain");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task OnResultExecutionAsync_WithErrorStatusCode_DoesNotChangeContentType()
    {
        // Arrange
        var filter = new SuccessContentTypeFilter();
        var httpContext = new DefaultHttpContext();
        httpContext.Response.StatusCode = 500;

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var resultContext = new ResultExecutingContext(
            actionContext,
            Array.Empty<IFilterMetadata>(),
            new ObjectResult(new { Message = "Error" }),
            null!);

        var nextCalled = false;
        ResultExecutionDelegate next = () =>
        {
            nextCalled = true;
            var executedContext = new ResultExecutedContext(
                resultContext,
                Array.Empty<IFilterMetadata>(),
                resultContext.Result,
                null!);
            return Task.FromResult(executedContext);
        };

        // Act
        await filter.OnResultExecutionAsync(resultContext, next);

        // Assert
        httpContext.Response.ContentType.Should().BeNull();
        nextCalled.Should().BeTrue();
    }
}