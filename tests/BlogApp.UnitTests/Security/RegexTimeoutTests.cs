using System.Diagnostics;
using System.Text.RegularExpressions;

namespace BlogApp.UnitTests.Security;

public class RegexTimeoutTests
{
    [Fact]
    public void FileSecurityConstants_RegexTimeout_ShouldBeReasonable()
    {
        // Arrange & Act & Assert
        FileSecurityConstants.RegexTimeout.Should().BePositive();
        FileSecurityConstants.RegexTimeout.Should().BeLessThan(TimeSpan.FromSeconds(1));
        FileSecurityConstants.RegexTimeout.TotalMilliseconds.Should().Be(100);
    }

    [Fact]
    public void FileSecurityConstants_DefaultRegexOptions_ShouldIncludeCompiled()
    {
        // Arrange & Act & Assert
        FileSecurityConstants.DefaultRegexOptions.Should().HaveFlag(RegexOptions.Compiled);
        FileSecurityConstants.DefaultRegexOptions.Should().HaveFlag(RegexOptions.IgnoreCase);
    }

    [Fact]
    public void Regex_WithTimeout_ShouldTimeoutOnMaliciousPattern()
    {
        // Arrange - A potentially catastrophic backtracking pattern
        var maliciousInput = new string('a', 1000) + "X";
        var evilPattern = @"^(a+)+$";

        // Act & Assert
        var action = () => Regex.IsMatch(maliciousInput, evilPattern,
            FileSecurityConstants.DefaultRegexOptions,
            FileSecurityConstants.RegexTimeout);

        action.Should().Throw<RegexMatchTimeoutException>();
    }

    [Theory]
    [InlineData(@"[<>""'&]", "test<script>alert('xss')</script>")]
    [InlineData(@"\s+", "multiple   spaces    here")]
    [InlineData(@"password=[^&]*", "username=test&password=secret&token=abc")]
    public void Regex_WithTimeout_ShouldWorkNormallyWithValidInput(string pattern, string input)
    {
        // Arrange & Act
        var result = Regex.Replace(input, pattern, "*",
            FileSecurityConstants.DefaultRegexOptions,
            FileSecurityConstants.RegexTimeout);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBe(input); // Should have been modified
    }

    [Fact]
    public void Regex_WithTimeout_ShouldHandleNormalPatternsQuickly()
    {
        // Arrange
        var normalInput = "This is a normal string with some spaces and characters.";
        var normalPattern = @"\s+";
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = Regex.Replace(normalInput, normalPattern, " ",
            FileSecurityConstants.DefaultRegexOptions,
            FileSecurityConstants.RegexTimeout);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNull();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(50); // Should be very fast
    }
}