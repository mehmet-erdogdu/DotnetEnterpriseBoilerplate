using BlogApp.Domain.Entities;
using BlogApp.Infrastructure.Repositories;

namespace BlogApp.UnitTests.Infrastructure.Repositories;

public class AuditLogRepositoryTests : BaseTestClass
{
    private AuditLogRepository? _repository;

    public AuditLogRepositoryTests()
    {
        var context = CreateDbContext();
        _repository = new AuditLogRepository(context);
    }

    [Fact]
    public async Task GetLogsForEntity_WithValidParameters_ShouldReturnLogs()
    {
        // Arrange
        var context = CreateDbContext();
        var auditLogs = new List<AuditLog>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TableName = "Posts",
                EntityId = Guid.NewGuid(),
                Action = "Insert",
                UserId = "test-user-id",
                Timestamp = DateTime.UtcNow,
                OldValues = null,
                NewValues = "{\"Title\":\"Test Post\"}"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TableName = "Posts",
                EntityId = Guid.NewGuid(), // Different entity
                Action = "Update",
                UserId = "test-user-id",
                Timestamp = DateTime.UtcNow.AddMinutes(-1),
                OldValues = "{\"Title\":\"Old Title\"}",
                NewValues = "{\"Title\":\"New Title\"}"
            }
        };

        await context.AuditLogs.AddRangeAsync(auditLogs);
        await context.SaveChangesAsync();

        var repository = new AuditLogRepository(context);
        var entityId = auditLogs[0].EntityId;

        // Act
        var result = await repository.GetLogsForEntity("Posts", entityId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        var log = result.First();
        log.TableName.Should().Be("Posts");
        log.EntityId.Should().Be(entityId);
        log.Action.Should().Be("Insert");
    }

    [Fact]
    public async Task GetLogsForEntity_WithNoMatchingLogs_ShouldReturnEmptyList()
    {
        // Arrange
        var context = CreateDbContext();
        var repository = new AuditLogRepository(context);
        
        // Act
        var result = await repository.GetLogsForEntity("NonExistentTable", Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLogsByUser_WithValidUserId_ShouldReturnUserLogs()
    {
        // Arrange
        var context = CreateDbContext();
        var userId = "test-user-id";
        var otherUserId = "other-user-id";
        
        var auditLogs = new List<AuditLog>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TableName = "Posts",
                EntityId = Guid.NewGuid(),
                Action = "Insert",
                UserId = userId,
                Timestamp = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TableName = "Todos",
                EntityId = Guid.NewGuid(),
                Action = "Update",
                UserId = otherUserId, // Different user
                Timestamp = DateTime.UtcNow.AddMinutes(-1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                TableName = "Files",
                EntityId = Guid.NewGuid(),
                Action = "Delete",
                UserId = userId, // Same user
                Timestamp = DateTime.UtcNow.AddMinutes(-2)
            }
        };

        await context.AuditLogs.AddRangeAsync(auditLogs);
        await context.SaveChangesAsync();

        var repository = new AuditLogRepository(context);

        // Act
        var result = await repository.GetLogsByUser(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(log => log.UserId == userId);
        result.Should().BeInDescendingOrder(log => log.Timestamp);
    }

    [Fact]
    public async Task GetLogsByUser_WithNoLogsForUser_ShouldReturnEmptyList()
    {
        // Arrange
        var context = CreateDbContext();
        var repository = new AuditLogRepository(context);
        
        // Act
        var result = await repository.GetLogsByUser("non-existent-user");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLogsByDateRange_WithValidDateRange_ShouldReturnLogsInRange()
    {
        // Arrange
        var context = CreateDbContext();
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow.AddHours(1);
        var beforeStartDate = startDate.AddHours(-1);
        var afterEndDate = endDate.AddHours(1);
        
        var auditLogs = new List<AuditLog>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TableName = "Posts",
                EntityId = Guid.NewGuid(),
                Action = "Insert",
                UserId = "test-user-id",
                Timestamp = startDate.AddHours(1) // Within range
            },
            new()
            {
                Id = Guid.NewGuid(),
                TableName = "Todos",
                EntityId = Guid.NewGuid(),
                Action = "Update",
                UserId = "test-user-id",
                Timestamp = startDate.AddMinutes(30) // Within range
            },
            new()
            {
                Id = Guid.NewGuid(),
                TableName = "Files",
                EntityId = Guid.NewGuid(),
                Action = "Delete",
                UserId = "test-user-id",
                Timestamp = beforeStartDate // Before range
            },
            new()
            {
                Id = Guid.NewGuid(),
                TableName = "Users",
                EntityId = Guid.NewGuid(),
                Action = "Insert",
                UserId = "test-user-id",
                Timestamp = afterEndDate // After range
            }
        };

        await context.AuditLogs.AddRangeAsync(auditLogs);
        await context.SaveChangesAsync();

        var repository = new AuditLogRepository(context);

        // Act
        var result = await repository.GetLogsByDateRange(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(log => log.Timestamp >= startDate && log.Timestamp <= endDate);
        result.Should().BeInDescendingOrder(log => log.Timestamp);
    }

    [Fact]
    public async Task GetLogsByDateRange_WithNoLogsInRange_ShouldReturnEmptyList()
    {
        // Arrange
        var context = CreateDbContext();
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = DateTime.UtcNow.AddDays(2);
        
        var auditLogs = new List<AuditLog>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TableName = "Posts",
                EntityId = Guid.NewGuid(),
                Action = "Insert",
                UserId = "test-user-id",
                Timestamp = DateTime.UtcNow.AddDays(-1) // Before range
            }
        };

        await context.AuditLogs.AddRangeAsync(auditLogs);
        await context.SaveChangesAsync();

        var repository = new AuditLogRepository(context);

        // Act
        var result = await repository.GetLogsByDateRange(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnAuditLog()
    {
        // Arrange
        var context = CreateDbContext();
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TableName = "Posts",
            EntityId = Guid.NewGuid(),
            Action = "Insert",
            UserId = "test-user-id",
            Timestamp = DateTime.UtcNow
        };

        await context.AuditLogs.AddAsync(auditLog);
        await context.SaveChangesAsync();

        var repository = new AuditLogRepository(context);

        // Act
        var result = await repository.GetByIdAsync(auditLog.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(auditLog.Id);
        result.TableName.Should().Be(auditLog.TableName);
        result.EntityId.Should().Be(auditLog.EntityId);
        result.Action.Should().Be(auditLog.Action);
        result.UserId.Should().Be(auditLog.UserId);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var context = CreateDbContext();
        var repository = new AuditLogRepository(context);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllAuditLogs()
    {
        // Arrange
        var context = CreateDbContext();
        var auditLogs = new List<AuditLog>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TableName = "Posts",
                EntityId = Guid.NewGuid(),
                Action = "Insert",
                UserId = "test-user-id",
                Timestamp = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TableName = "Todos",
                EntityId = Guid.NewGuid(),
                Action = "Update",
                UserId = "test-user-id",
                Timestamp = DateTime.UtcNow.AddMinutes(-1)
            }
        };

        await context.AuditLogs.AddRangeAsync(auditLogs);
        await context.SaveChangesAsync();

        var repository = new AuditLogRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task AddAsync_ShouldAddAuditLog()
    {
        // Arrange
        var context = CreateDbContext();
        var repository = new AuditLogRepository(context);
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TableName = "Posts",
            EntityId = Guid.NewGuid(),
            Action = "Insert",
            UserId = "test-user-id",
            Timestamp = DateTime.UtcNow
        };

        // Act
        await repository.AddAsync(auditLog);
        await context.SaveChangesAsync();

        // Assert
        var result = await context.AuditLogs.FindAsync(auditLog.Id);
        result.Should().NotBeNull();
        result!.Id.Should().Be(auditLog.Id);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _repository = null;
        }
        base.Dispose(disposing);
    }
}