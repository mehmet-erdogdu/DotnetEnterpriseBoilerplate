using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;

namespace BlogApp.UnitTests.API.Middleware;

public class ProblemDetailsResultFilterTests : BaseTestClass
{
    [Fact]
    public async Task OnResultExecutionAsync_WithObjectResultAndErrorStatusCode_WrapsInApiResponse()
    {
        // Arrange
        var filter = new ProblemDetailsResultFilter();
        var httpContext = new DefaultHttpContext();
        httpContext.Response.StatusCode = 400;

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var resultContext = new ResultExecutingContext(
            actionContext,
            Array.Empty<IFilterMetadata>(),
            new ObjectResult(new { Message = "Bad request" }) { StatusCode = 400 },
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
        resultContext.Result.Should().BeOfType<ObjectResult>();
        var objectResult = resultContext.Result as ObjectResult;
        objectResult!.Value.Should().BeAssignableTo<ApiResponse<object>>();
        var apiResponse = objectResult.Value as ApiResponse<object>;
        apiResponse!.IsSuccess.Should().BeFalse();
        apiResponse.Error!.Message.Should().Be("Bad request");
        apiResponse.Error.Code.Should().Be("BAD_REQUEST");
        httpContext.Response.ContentType.Should().Be("application/json");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task OnResultExecutionAsync_WithStatusCodeResultAndErrorStatusCode_WrapsInApiResponse()
    {
        // Arrange
        var filter = new ProblemDetailsResultFilter();
        var httpContext = new DefaultHttpContext();
        httpContext.Response.StatusCode = 404;

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var resultContext = new ResultExecutingContext(
            actionContext,
            Array.Empty<IFilterMetadata>(),
            new StatusCodeResult(404),
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
        resultContext.Result.Should().BeOfType<ObjectResult>();
        var objectResult = resultContext.Result as ObjectResult;
        objectResult!.Value.Should().BeAssignableTo<ApiResponse<object>>();
        var apiResponse = objectResult.Value as ApiResponse<object>;
        apiResponse!.IsSuccess.Should().BeFalse();
        apiResponse.Error!.Message.Should().Be("Not found");
        apiResponse.Error.Code.Should().Be("NOT_FOUND");
        httpContext.Response.ContentType.Should().Be("application/json");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task OnResultExecutionAsync_WithSuccessStatusCode_DoesNotWrap()
    {
        // Arrange
        var filter = new ProblemDetailsResultFilter();
        var httpContext = new DefaultHttpContext();
        httpContext.Response.StatusCode = 200;

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var resultContext = new ResultExecutingContext(
            actionContext,
            Array.Empty<IFilterMetadata>(),
            new ObjectResult(new { Message = "Success" }) { StatusCode = 200 },
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
        resultContext.Result.Should().BeOfType<ObjectResult>();
        var objectResult = resultContext.Result as ObjectResult;
        // For success status codes, the filter should not wrap the result
        objectResult!.Value.Should().NotBeAssignableTo<ApiResponse<object>>();
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task OnResultExecutionAsync_WithAlreadyWrappedApiResponse_DoesNotWrapAgain()
    {
        // Arrange
        var filter = new ProblemDetailsResultFilter();
        var httpContext = new DefaultHttpContext();
        httpContext.Response.StatusCode = 400;

        var apiResponse = ApiResponse<object>.Failure("Already wrapped");
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var resultContext = new ResultExecutingContext(
            actionContext,
            Array.Empty<IFilterMetadata>(),
            new ObjectResult(apiResponse) { StatusCode = 400 },
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
        resultContext.Result.Should().BeOfType<ObjectResult>();
        var objectResult = resultContext.Result as ObjectResult;
        objectResult!.Value.Should().BeSameAs(apiResponse);
        nextCalled.Should().BeTrue();
    }
}