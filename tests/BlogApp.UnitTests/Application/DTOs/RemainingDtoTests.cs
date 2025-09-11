using BlogApp.Application.DTOs;
using Xunit;

namespace BlogApp.UnitTests.Application.DTOs;

public class DashboardStatisticsDtoTests
{
    [Fact]
    public void DashboardStatisticsDto_Should_Set_Properties_Correctly()
    {
        // Arrange
        var postsCount = 10;
        var todosCount = 5;
        var filesCount = 3;

        // Act
        var dashboardStats = new DashboardStatisticsDto
        {
            PostsCount = postsCount,
            TodosCount = todosCount,
            FilesCount = filesCount
        };

        // Assert
        Assert.Equal(postsCount, dashboardStats.PostsCount);
        Assert.Equal(todosCount, dashboardStats.TodosCount);
        Assert.Equal(filesCount, dashboardStats.FilesCount);
    }

    [Fact]
    public void DashboardStatisticsDto_Should_Have_Default_Values()
    {
        // Act
        var dashboardStats = new DashboardStatisticsDto();

        // Assert
        Assert.Equal(0, dashboardStats.PostsCount);
        Assert.Equal(0, dashboardStats.TodosCount);
        Assert.Equal(0, dashboardStats.FilesCount);
    }
}

public class LoginResponseDtoTests
{
    [Fact]
    public void LoginResponseDto_Should_Set_Properties_Correctly()
    {
        // Arrange
        var accessToken = "test-access-token";
        var refreshToken = "test-refresh-token";
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var tokenType = "Bearer";

        // Act
        var loginResponse = new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            TokenType = tokenType
        };

        // Assert
        Assert.Equal(accessToken, loginResponse.AccessToken);
        Assert.Equal(refreshToken, loginResponse.RefreshToken);
        Assert.Equal(expiresAt, loginResponse.ExpiresAt);
        Assert.Equal(tokenType, loginResponse.TokenType);
    }

    [Fact]
    public void LoginResponseDto_Should_Have_Default_Values()
    {
        // Act
        var loginResponse = new LoginResponseDto();

        // Assert
        Assert.Equal(string.Empty, loginResponse.AccessToken);
        Assert.Equal(string.Empty, loginResponse.RefreshToken);
        Assert.Equal(default(DateTime), loginResponse.ExpiresAt);
        Assert.Equal("Bearer", loginResponse.TokenType);
    }
}

public class PaginationDtoTests
{
    [Fact]
    public void PaginationDto_Should_Set_Properties_Correctly()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;
        var search = "test search";

        // Act
        var pagination = new PaginationDto
        {
            Page = page,
            PageSize = pageSize,
            Search = search
        };

        // Assert
        Assert.Equal(page, pagination.Page);
        Assert.Equal(pageSize, pagination.PageSize);
        Assert.Equal(search, pagination.Search);
    }

    [Fact]
    public void PaginationDto_Should_Allow_Null_Values()
    {
        // Act
        var pagination = new PaginationDto
        {
            Page = null,
            PageSize = null,
            Search = null
        };

        // Assert
        Assert.Null(pagination.Page);
        Assert.Null(pagination.PageSize);
        Assert.Null(pagination.Search);
    }

    [Fact]
    public void PaginationDto_Should_Have_Default_Values()
    {
        // Act
        var pagination = new PaginationDto();

        // Assert
        Assert.Null(pagination.Page);
        Assert.Null(pagination.PageSize);
        Assert.Null(pagination.Search);
    }
}

public class RefreshTokenDtoTests
{
    [Fact]
    public void RefreshTokenDto_Should_Set_Properties_Correctly()
    {
        // Arrange
        var refreshToken = "test-refresh-token";

        // Act
        var refreshTokenDto = new RefreshTokenDto
        {
            RefreshToken = refreshToken
        };

        // Assert
        Assert.Equal(refreshToken, refreshTokenDto.RefreshToken);
    }

    [Fact]
    public void RefreshTokenDto_Should_Have_Default_Values()
    {
        // Act
        var refreshTokenDto = new RefreshTokenDto();

        // Assert
        Assert.Equal(string.Empty, refreshTokenDto.RefreshToken);
    }
}