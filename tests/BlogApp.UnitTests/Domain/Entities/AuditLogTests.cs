namespace BlogApp.UnitTests.Domain.Entities;

public class AuditLogTests : BaseTestClass
{
    [Fact]
    public void AuditLog_Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tableName = "Posts";
        var action = "Create";
        var userId = "user-123";
        var timestamp = DateTime.UtcNow;
        var oldValues = "{\"title\":\"Old Title\"}";
        var newValues = "{\"title\":\"New Title\"}";
        var changedColumns = "Title";

        // Act
        var auditLog = new AuditLog
        {
            Id = id,
            TableName = tableName,
            Action = action,
            UserId = userId,
            Timestamp = timestamp,
            OldValues = oldValues,
            NewValues = newValues,
            ChangedColumns = changedColumns
        };

        // Assert
        auditLog.Id.Should().Be(id);
        auditLog.TableName.Should().Be(tableName);
        auditLog.Action.Should().Be(action);
        auditLog.UserId.Should().Be(userId);
        auditLog.Timestamp.Should().Be(timestamp);
        auditLog.OldValues.Should().Be(oldValues);
        auditLog.NewValues.Should().Be(newValues);
        auditLog.ChangedColumns.Should().Be(changedColumns);
    }

    [Fact]
    public void AuditLog_DefaultValues_AreCorrect()
    {
        // Act
        var auditLog = new AuditLog
        {
            Timestamp = DateTime.UtcNow
        };

        // Assert
        auditLog.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("Posts", "Create")]
    [InlineData("Todos", "Update")]
    [InlineData("Users", "Delete")]
    public void AuditLog_WithDifferentActions_SetsPropertiesCorrectly(string tableName, string action)
    {
        // Act
        var auditLog = new AuditLog
        {
            TableName = tableName,
            Action = action,
            UserId = "test-user",
            Timestamp = DateTime.UtcNow
        };

        // Assert
        auditLog.TableName.Should().Be(tableName);
        auditLog.Action.Should().Be(action);
        auditLog.UserId.Should().Be("test-user");
    }
}