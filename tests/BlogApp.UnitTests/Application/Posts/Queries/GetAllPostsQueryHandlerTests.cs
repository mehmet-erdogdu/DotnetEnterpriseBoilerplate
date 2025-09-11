namespace BlogApp.UnitTests.Application.Posts.Queries;

public class GetAllPostsQueryHandlerTests : BaseTestClass
{
    [Fact]
    public void Should_CreateHandler_Successfully()
    {
        // Arrange
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        // Act
        var handler = new GetAllPostsQueryHandler(mockUnitOfWork.Object);

        // Assert
        handler.Should().NotBeNull();
    }

    [Fact]
    public void Should_AccessUnitOfWork_Successfully()
    {
        // Arrange
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockPostRepository = new Mock<IPostRepository>();

        mockUnitOfWork.Setup(x => x.Posts).Returns(mockPostRepository.Object);

        var handler = new GetAllPostsQueryHandler(mockUnitOfWork.Object);

        // Act - Access the Posts property to verify setup
        var posts = mockUnitOfWork.Object.Posts;

        // Assert
        posts.Should().NotBeNull();
        mockUnitOfWork.Verify(x => x.Posts, Times.Once);
    }
}