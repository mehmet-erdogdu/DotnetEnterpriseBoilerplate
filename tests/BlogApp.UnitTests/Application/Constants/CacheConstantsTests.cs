using BlogApp.Application.Constants;
using Xunit;

namespace BlogApp.UnitTests.Application.Constants;

public class CacheConstantsTests
{
    [Fact]
    public void CacheConstants_Should_Have_Expected_Prefixes()
    {
        // Assert
        Assert.Equal("posts", CacheConstants.PostsPrefix);
        Assert.Equal("todos", CacheConstants.TodosPrefix);
        Assert.Equal("user", CacheConstants.UserPrefix);
        Assert.Equal("auth", CacheConstants.AuthPrefix);
    }

    [Fact]
    public void CacheConstants_Keys_Should_Generate_Correct_Keys()
    {
        // Arrange
        var testId = Guid.NewGuid();
        var testUserId = "test-user-id";
        var page = 1;
        var pageSize = 10;

        // Act
        var postByIdKey = CacheConstants.Keys.PostById(testId);
        var postsListKey = CacheConstants.Keys.PostsList(page, pageSize);
        var todoByIdKey = CacheConstants.Keys.TodoById(testId);
        var todosListKey = CacheConstants.Keys.TodosList(page, pageSize);
        var userByIdKey = CacheConstants.Keys.UserById(testUserId);
        var userPostsKey = CacheConstants.Keys.UserPosts(testUserId);
        var refreshTokenKey = CacheConstants.Keys.RefreshToken("test-token");

        // Assert
        Assert.Equal($"posts:{testId}", postByIdKey);
        Assert.Equal($"posts:list:{page}:{pageSize}", postsListKey);
        Assert.Equal($"todos:{testId}", todoByIdKey);
        Assert.Equal($"todos:list:{page}:{pageSize}", todosListKey);
        Assert.Equal($"user:{testUserId}", userByIdKey);
        Assert.Equal($"user:{testUserId}:posts", userPostsKey);
        Assert.Equal("auth:refresh:test-token", refreshTokenKey);
    }

    [Fact]
    public void CacheConstants_Expiration_Should_Have_Expected_Values()
    {
        // Assert
        Assert.Equal(TimeSpan.FromMinutes(30), CacheConstants.Expiration.Posts);
        Assert.Equal(TimeSpan.FromMinutes(15), CacheConstants.Expiration.Todos);
        Assert.Equal(TimeSpan.FromMinutes(60), CacheConstants.Expiration.User);
        Assert.Equal(TimeSpan.FromMinutes(5), CacheConstants.Expiration.Auth);
    }

    [Fact]
    public void CacheConstants_Keys_Should_Handle_Null_Parameters()
    {
        // Act
        var postsListKeyWithNullPage = CacheConstants.Keys.PostsList(null, 10);
        var postsListKeyWithNullPageSize = CacheConstants.Keys.PostsList(1, null);
        var postsListKeyWithBothNull = CacheConstants.Keys.PostsList(null, null);

        // Assert
        Assert.Equal("posts:list:0:10", postsListKeyWithNullPage);
        Assert.Equal("posts:list:1:10", postsListKeyWithNullPageSize);
        Assert.Equal("posts:list:0:10", postsListKeyWithBothNull);
    }
}