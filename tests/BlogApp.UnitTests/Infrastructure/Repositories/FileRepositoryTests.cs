using BlogApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.UnitTests.Infrastructure.Repositories;

public class FileRepositoryTests : BaseTestClass
{
    private new readonly ApplicationDbContext _context = null!;
    private readonly FileRepository _fileRepository = null!;

    public FileRepositoryTests()
    {
        _context = CreateDbContext();
        _fileRepository = new FileRepository(_context);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _context.Dispose();
        base.Dispose(disposing);
    }

    [Fact]
    public async Task GetByFileNameAsync_WithExistingFileName_ReturnsFile()
    {
        // Arrange
        var file = TestHelper.TestData.CreateTestFile();
        await _context.Files.AddAsync(file);
        await _context.SaveChangesAsync();

        // Act
        var result = await _fileRepository.GetByFileNameAsync(file.FileName);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(file.Id);
        result.FileName.Should().Be(file.FileName);
    }

    [Fact]
    public async Task GetByFileNameAsync_WithNonExistentFileName_ReturnsNull()
    {
        // Arrange
        var fileName = "non-existent-file.txt";

        // Act
        var result = await _fileRepository.GetByFileNameAsync(fileName);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUploadedByIdAsync_WithValidUserId_ReturnsUserFiles()
    {
        // Arrange
        var user = TestHelper.TestData.CreateTestUser();
        await _context.Users.AddAsync(user);

        var file1 = TestHelper.TestData.CreateTestFile(user.Id);
        var file2 = TestHelper.TestData.CreateTestFile(user.Id, Guid.NewGuid());
        var file3 = TestHelper.TestData.CreateTestFile("other-user-id", Guid.NewGuid());

        await _context.Files.AddRangeAsync(file1, file2, file3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _fileRepository.GetByUploadedByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(f => f.UploadedById == user.Id);
    }

    [Fact]
    public async Task GetByUploadedByIdAsync_WithNonExistentUserId_ReturnsEmptyList()
    {
        // Arrange
        var userId = "non-existent-user-id";

        // Act
        var result = await _fileRepository.GetByUploadedByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ExistsByFileNameAsync_WithExistingFileName_ReturnsTrue()
    {
        // Arrange
        var file = TestHelper.TestData.CreateTestFile();
        await _context.Files.AddAsync(file);
        await _context.SaveChangesAsync();

        // Act
        var result = await _fileRepository.ExistsByFileNameAsync(file.FileName);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByFileNameAsync_WithNonExistentFileName_ReturnsFalse()
    {
        // Arrange
        var fileName = "non-existent-file.txt";

        // Act
        var result = await _fileRepository.ExistsByFileNameAsync(fileName);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetCountByUserIdAsync_WithValidUserId_ReturnsCorrectCount()
    {
        // Arrange
        var user = TestHelper.TestData.CreateTestUser();
        await _context.Users.AddAsync(user);

        var file1 = TestHelper.TestData.CreateTestFile(user.Id);
        var file2 = TestHelper.TestData.CreateTestFile(user.Id, Guid.NewGuid());
        var file3 = TestHelper.TestData.CreateTestFile("other-user-id", Guid.NewGuid());

        await _context.Files.AddRangeAsync(file1, file2, file3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _fileRepository.GetCountByUserIdAsync(user.Id);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task GetCountByUserIdAsync_WithNonExistentUserId_ReturnsZero()
    {
        // Arrange
        var userId = "non-existent-user-id";

        // Act
        var result = await _fileRepository.GetCountByUserIdAsync(userId);

        // Assert
        result.Should().Be(0);
    }
}