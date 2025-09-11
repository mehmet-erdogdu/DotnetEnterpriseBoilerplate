using BlogApp.Application.DTOs;
using Xunit;

namespace BlogApp.UnitTests.Application.DTOs;

public class RoleDtoTests
{
    [Fact]
    public void RoleDto_Should_Set_Properties_Correctly()
    {
        // Arrange
        var id = "test-role-id";
        var name = "TestRole";
        var normalizedName = "TESTROLE";
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow.AddHours(1);

        // Act
        var roleDto = new RoleDto
        {
            Id = id,
            Name = name,
            NormalizedName = normalizedName,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        Assert.Equal(id, roleDto.Id);
        Assert.Equal(name, roleDto.Name);
        Assert.Equal(normalizedName, roleDto.NormalizedName);
        Assert.Equal(createdAt, roleDto.CreatedAt);
        Assert.Equal(updatedAt, roleDto.UpdatedAt);
    }

    [Fact]
    public void RoleDto_Should_Have_Default_Values()
    {
        // Act
        var roleDto = new RoleDto();

        // Assert
        Assert.Equal(string.Empty, roleDto.Id);
        Assert.Equal(string.Empty, roleDto.Name);
        Assert.Equal(string.Empty, roleDto.NormalizedName);
        Assert.Null(roleDto.CreatedAt);
        Assert.Null(roleDto.UpdatedAt);
    }

    [Fact]
    public void RoleDto_Should_Allow_Empty_Values()
    {
        // Act
        var roleDto = new RoleDto
        {
            Id = string.Empty,
            Name = string.Empty,
            NormalizedName = string.Empty,
            CreatedAt = null,
            UpdatedAt = null
        };

        // Assert
        Assert.Equal(string.Empty, roleDto.Id);
        Assert.Equal(string.Empty, roleDto.Name);
        Assert.Equal(string.Empty, roleDto.NormalizedName);
        Assert.Null(roleDto.CreatedAt);
        Assert.Null(roleDto.UpdatedAt);
    }
}

public class RoleClaimDtoTests
{
    [Fact]
    public void RoleClaimDto_Should_Set_Properties_Correctly()
    {
        // Arrange
        var id = 1;
        var roleId = "test-role-id";
        var claimType = "TestClaimType";
        var claimValue = "TestClaimValue";
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow.AddHours(1);

        // Act
        var roleClaimDto = new RoleClaimDto
        {
            Id = id,
            RoleId = roleId,
            ClaimType = claimType,
            ClaimValue = claimValue,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        Assert.Equal(id, roleClaimDto.Id);
        Assert.Equal(roleId, roleClaimDto.RoleId);
        Assert.Equal(claimType, roleClaimDto.ClaimType);
        Assert.Equal(claimValue, roleClaimDto.ClaimValue);
        Assert.Equal(createdAt, roleClaimDto.CreatedAt);
        Assert.Equal(updatedAt, roleClaimDto.UpdatedAt);
    }

    [Fact]
    public void RoleClaimDto_Should_Have_Default_Values()
    {
        // Act
        var roleClaimDto = new RoleClaimDto();

        // Assert
        Assert.Equal(0, roleClaimDto.Id);
        Assert.Equal(string.Empty, roleClaimDto.RoleId);
        Assert.Equal(string.Empty, roleClaimDto.ClaimType);
        Assert.Equal(string.Empty, roleClaimDto.ClaimValue);
        Assert.Null(roleClaimDto.CreatedAt);
        Assert.Null(roleClaimDto.UpdatedAt);
    }

    [Fact]
    public void RoleClaimDto_Should_Allow_Empty_Values()
    {
        // Act
        var roleClaimDto = new RoleClaimDto
        {
            Id = 0,
            RoleId = string.Empty,
            ClaimType = string.Empty,
            ClaimValue = string.Empty,
            CreatedAt = null,
            UpdatedAt = null
        };

        // Assert
        Assert.Equal(0, roleClaimDto.Id);
        Assert.Equal(string.Empty, roleClaimDto.RoleId);
        Assert.Equal(string.Empty, roleClaimDto.ClaimType);
        Assert.Equal(string.Empty, roleClaimDto.ClaimValue);
        Assert.Null(roleClaimDto.CreatedAt);
        Assert.Null(roleClaimDto.UpdatedAt);
    }
}