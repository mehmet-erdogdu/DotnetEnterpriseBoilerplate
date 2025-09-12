namespace BlogApp.UnitTests.API.Controllers;

public class SystemControllerTests : BaseTestClass
{
    [Fact]
    public void Get_ReturnsConfigurationValues()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "React:ApiUrl", "https://localhost:7266" },
            { "React:Title", "Blog App" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var controller = new SystemController(configuration);

        // Act
        var result = controller.Get();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().ContainKey("ApiUrl");
        result.Data.Should().ContainKey("Title");
        result.Data!["ApiUrl"].Should().Be("https://localhost:7266");
        result.Data["Title"].Should().Be("Blog App");
    }

    [Fact]
    public void Get_ReturnsEmptyDictionary_WhenNoReactSection()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Other:Setting", "value" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var controller = new SystemController(configuration);

        // Act
        var result = controller.Get();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty();
    }
}