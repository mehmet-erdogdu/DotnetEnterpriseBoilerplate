using BlogApp.Infrastructure.Repositories;

namespace BlogApp.UnitTests.Infrastructure.Repositories;

public class UnitOfWorkTests : BaseTestClass
{
    private new ApplicationDbContext _context = null!;
    private UnitOfWork _unitOfWork = null!;

    public UnitOfWorkTests()
    {
        _context = CreateDbContext();
        _unitOfWork = new UnitOfWork(_context);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _unitOfWork.Dispose();
            _context.Dispose();
        }
        base.Dispose(disposing);
    }

    [Fact]
    public void Posts_Property_ReturnsPostRepository()
    {
        // Act
        var posts = _unitOfWork.Posts;

        // Assert
        posts.Should().NotBeNull();
        posts.Should().BeAssignableTo<IPostRepository>();
    }

    [Fact]
    public void Todos_Property_ReturnsTodoRepository()
    {
        // Act
        var todos = _unitOfWork.Todos;

        // Assert
        todos.Should().NotBeNull();
        todos.Should().BeAssignableTo<ITodoRepository>();
    }

    [Fact]
    public void AuditLogs_Property_ReturnsAuditLogRepository()
    {
        // Act
        var auditLogs = _unitOfWork.AuditLogs;

        // Assert
        auditLogs.Should().NotBeNull();
        auditLogs.Should().BeAssignableTo<IAuditLogRepository>();
    }

    [Fact]
    public void PasswordHistory_Property_ReturnsPasswordHistoryRepository()
    {
        // Act
        var passwordHistory = _unitOfWork.PasswordHistory;

        // Assert
        passwordHistory.Should().NotBeNull();
        passwordHistory.Should().BeAssignableTo<IPasswordHistoryRepository>();
    }

    [Fact]
    public void RefreshTokens_Property_ReturnsRefreshTokenRepository()
    {
        // Act
        var refreshTokens = _unitOfWork.RefreshTokens;

        // Assert
        refreshTokens.Should().NotBeNull();
        refreshTokens.Should().BeAssignableTo<IRefreshTokenRepository>();
    }

    [Fact]
    public async Task SaveChangesAsync_WithChanges_ReturnsTrue()
    {
        // Arrange
        var todo = TestHelper.TestData.CreateTestTodo();
        _context.Todos.Add(todo);

        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SaveChangesAsync_WithoutChanges_ReturnsFalse()
    {
        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().BeFalse();
    }

    // Skip transaction tests for in-memory database since it doesn't support transactions
    // In a real database scenario, these tests would be valuable
}
