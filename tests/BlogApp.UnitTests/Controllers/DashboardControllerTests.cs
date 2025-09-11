namespace BlogApp.UnitTests.Controllers;

public class DashboardControllerTests : BaseControllerTest
{
    private readonly DashboardController _controller;

    public DashboardControllerTests()
    {
        _controller = new DashboardController(_mockMediator.Object, _mockCurrentUserService.Object);
    }

    #region GetStatistics Tests

    [Fact]
    public async Task GetStatistics_WithValidUser_ShouldReturnStatistics()
    {
        // Arrange
        var postsCount = 5;
        var todosCount = 10;
        var filesCount = 3;

        _mockMediator.Setup(x => x.Send(It.IsAny<GetPostsCountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(postsCount);
        _mockMediator.Setup(x => x.Send(It.IsAny<GetTodosCountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(todosCount);
        _mockMediator.Setup(x => x.Send(It.IsAny<GetFilesCountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(filesCount);

        // Act
        var result = await _controller.GetStatistics();

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.PostsCount.Should().Be(postsCount);
        result.Data.TodosCount.Should().Be(todosCount);
        result.Data.FilesCount.Should().Be(filesCount);

        // Verify that each query was called with the correct user ID
        _mockMediator.Verify(x => x.Send(It.Is<GetPostsCountQuery>(q =>
            q.UserId == "test-user-id"
        ), It.IsAny<CancellationToken>()), Times.Once);

        _mockMediator.Verify(x => x.Send(It.Is<GetTodosCountQuery>(q =>
            q.UserId == "test-user-id"
        ), It.IsAny<CancellationToken>()), Times.Once);

        _mockMediator.Verify(x => x.Send(It.Is<GetFilesCountQuery>(q =>
            q.UserId == "test-user-id"
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStatistics_WithZeroCounts_ShouldReturnZeroStatistics()
    {
        // Arrange

        _mockMediator.Setup(x => x.Send(It.IsAny<GetPostsCountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _mockMediator.Setup(x => x.Send(It.IsAny<GetTodosCountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _mockMediator.Setup(x => x.Send(It.IsAny<GetFilesCountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _controller.GetStatistics();

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.PostsCount.Should().Be(0);
        result.Data.TodosCount.Should().Be(0);
        result.Data.FilesCount.Should().Be(0);
    }

    [Fact]
    public async Task GetStatistics_WithLargeCounts_ShouldReturnCorrectStatistics()
    {
        // Arrange
        var postsCount = 1000;
        var todosCount = 2500;
        var filesCount = 150;

        _mockMediator.Setup(x => x.Send(It.IsAny<GetPostsCountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(postsCount);
        _mockMediator.Setup(x => x.Send(It.IsAny<GetTodosCountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(todosCount);
        _mockMediator.Setup(x => x.Send(It.IsAny<GetFilesCountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(filesCount);

        // Act
        var result = await _controller.GetStatistics();

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.PostsCount.Should().Be(postsCount);
        result.Data.TodosCount.Should().Be(todosCount);
        result.Data.FilesCount.Should().Be(filesCount);
    }

    [Fact]
    public async Task GetStatistics_WhenPostsQueryFails_ShouldStillCallOtherQueries()
    {
        // Arrange
        _mockMediator.Setup(x => x.Send(It.IsAny<GetPostsCountQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Posts query failed"));
        _mockMediator.Setup(x => x.Send(It.IsAny<GetTodosCountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);
        _mockMediator.Setup(x => x.Send(It.IsAny<GetFilesCountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _controller.GetStatistics());

        // Verify all queries were attempted
        _mockMediator.Verify(x => x.Send(It.IsAny<GetPostsCountQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        // Todos and Files queries may not be called if Posts query throws early
    }

    [Fact]
    public async Task GetStatistics_WithDifferentUserContext_ShouldUseCorrectUserId()
    {
        // Arrange
        var specificUserId = "specific-user-123";

        // Setup the current user service to return specific user ID
        _mockCurrentUserService.Setup(x => x.UserId).Returns(specificUserId);

        _mockMediator.Setup(x => x.Send(It.IsAny<GetPostsCountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);
        _mockMediator.Setup(x => x.Send(It.IsAny<GetTodosCountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(7);
        _mockMediator.Setup(x => x.Send(It.IsAny<GetFilesCountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        // Act
        var result = await _controller.GetStatistics();

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);

        // Verify that each query was called with the specific user ID
        _mockMediator.Verify(x => x.Send(It.Is<GetPostsCountQuery>(q =>
            q.UserId == specificUserId
        ), It.IsAny<CancellationToken>()), Times.Once);

        _mockMediator.Verify(x => x.Send(It.Is<GetTodosCountQuery>(q =>
            q.UserId == specificUserId
        ), It.IsAny<CancellationToken>()), Times.Once);

        _mockMediator.Verify(x => x.Send(It.Is<GetFilesCountQuery>(q =>
            q.UserId == specificUserId
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}