using BlogApp.Application.Common.Behaviors;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace BlogApp.UnitTests.Application.Common.Behaviors;

public class ValidationBehaviorTests : BaseApplicationTest
{
    private readonly Mock<IValidator<TestRequest>> _mockValidator;
    private readonly ValidationBehavior<TestRequest, TestResponse> _validationBehavior;
    private readonly List<IValidator<TestRequest>> _validators;

    public ValidationBehaviorTests()
    {
        _mockValidator = new Mock<IValidator<TestRequest>>();
        _validators = new List<IValidator<TestRequest>> { _mockValidator.Object };
        _validationBehavior = new ValidationBehavior<TestRequest, TestResponse>(_validators);
    }

    [Fact]
    public async Task Handle_WhenNoValidators_ShouldCallNext()
    {
        // Arrange
        var emptyValidators = new List<IValidator<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(emptyValidators);
        var request = new TestRequest { Name = "Test" };
        var response = new TestResponse { Message = "Success" };
        var nextCalled = false;

        RequestHandlerDelegate<TestResponse> next = cancellationToken =>
        {
            nextCalled = true;
            return Task.FromResult(response);
        };

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be(response);
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenValidationPasses_ShouldCallNext()
    {
        // Arrange
        var request = new TestRequest { Name = "Test" };
        var response = new TestResponse { Message = "Success" };
        var validationResult = new ValidationResult();

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), CancellationToken.None))
            .ReturnsAsync(validationResult);

        var nextCalled = false;
        RequestHandlerDelegate<TestResponse> next = cancellationToken =>
        {
            nextCalled = true;
            return Task.FromResult(response);
        };

        // Act
        var result = await _validationBehavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be(response);
        nextCalled.Should().BeTrue();
        _mockValidator.Verify(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ShouldThrowValidationException()
    {
        // Arrange
        var request = new TestRequest { Name = "" };
        var validationFailure = new ValidationFailure("Name", "Name is required");
        var validationResult = new ValidationResult(new List<ValidationFailure> { validationFailure });

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), CancellationToken.None))
            .ReturnsAsync(validationResult);

        RequestHandlerDelegate<TestResponse> next = cancellationToken => Task.FromResult(new TestResponse());

        // Act
        Func<Task> act = () => _validationBehavior.Handle(request, next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.Count() == 1 && ex.Errors.First().PropertyName == "Name");
        _mockValidator.Verify(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMultipleValidatorsAndOneFails_ShouldThrowValidationException()
    {
        // Arrange
        var request = new TestRequest { Name = "" };
        var mockValidator2 = new Mock<IValidator<TestRequest>>();

        var validators = new List<IValidator<TestRequest>> { _mockValidator.Object, mockValidator2.Object };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators);

        var validationResult1 = new ValidationResult();
        var validationFailure = new ValidationFailure("Name", "Name is required");
        var validationResult2 = new ValidationResult(new List<ValidationFailure> { validationFailure });

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), CancellationToken.None))
            .ReturnsAsync(validationResult1);

        mockValidator2.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), CancellationToken.None))
            .ReturnsAsync(validationResult2);

        RequestHandlerDelegate<TestResponse> next = cancellationToken => Task.FromResult(new TestResponse());

        // Act
        Func<Task> act = () => behavior.Handle(request, next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.Count() == 1 && ex.Errors.First().PropertyName == "Name");

        _mockValidator.Verify(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), CancellationToken.None), Times.Once);
        mockValidator2.Verify(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), CancellationToken.None), Times.Once);
    }
}

public class TestRequest : IRequest<TestResponse>
{
    public string Name { get; set; } = string.Empty;
}

public class TestResponse
{
    public string Message { get; set; } = string.Empty;
}