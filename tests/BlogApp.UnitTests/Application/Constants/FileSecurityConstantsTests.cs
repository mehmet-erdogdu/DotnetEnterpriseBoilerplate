using BlogApp.Application.Constants;
using System.Collections.Immutable;
using Xunit;

namespace BlogApp.UnitTests.Application.Constants;

public class FileSecurityConstantsTests
{
    [Fact]
    public void FileSecurityConstants_Should_Have_Expected_FileSize_Limits()
    {
        // Assert
        Assert.Equal(100 * 1024 * 1024L, FileSecurityConstants.MaxFileSize);
        Assert.Equal(1L, FileSecurityConstants.MinFileSize);
    }

    [Fact]
    public void FileSecurityConstants_Should_Have_Expected_FileName_Limits()
    {
        // Assert
        Assert.Equal(255, FileSecurityConstants.MaxFileNameLength);
        Assert.Equal(1, FileSecurityConstants.MinFileNameLength);
    }

    [Fact]
    public void FileSecurityConstants_Should_Have_Expected_PresignedUrl_Expiry()
    {
        // Assert
        Assert.Equal(15, FileSecurityConstants.PresignedUrlExpiryMinutes);
    }

    [Fact]
    public void FileSecurityConstants_Should_Have_Expected_RateLimiting_Values()
    {
        // Assert
        Assert.Equal(100, FileSecurityConstants.MaxUploadRequestsPerHour);
        Assert.Equal(1000, FileSecurityConstants.MaxUploadRequestsPerDay);
    }

    [Fact]
    public void FileSecurityConstants_Should_Have_Expected_Regex_Settings()
    {
        // Assert
        Assert.Equal(TimeSpan.FromMilliseconds(100), FileSecurityConstants.RegexTimeout);
        Assert.Equal(System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled, 
            FileSecurityConstants.DefaultRegexOptions);
    }

    [Fact]
    public void FileSecurityConstants_AllowedExtensions_Should_Contain_Expected_Values()
    {
        // Assert
        Assert.Contains(".jpg", FileSecurityConstants.AllowedExtensions);
        Assert.Contains(".jpeg", FileSecurityConstants.AllowedExtensions);
        Assert.Contains(".png", FileSecurityConstants.AllowedExtensions);
        Assert.Contains(".gif", FileSecurityConstants.AllowedExtensions);
        Assert.Contains(".pdf", FileSecurityConstants.AllowedExtensions);
        Assert.Contains(".doc", FileSecurityConstants.AllowedExtensions);
        Assert.Contains(".docx", FileSecurityConstants.AllowedExtensions);
        Assert.Contains(".txt", FileSecurityConstants.AllowedExtensions);
        Assert.Contains(".zip", FileSecurityConstants.AllowedExtensions);
        Assert.Contains(".mp4", FileSecurityConstants.AllowedExtensions);
        
        // Ensure it's immutable
        Assert.IsAssignableFrom<ImmutableHashSet<string>>(FileSecurityConstants.AllowedExtensions);
    }

    [Fact]
    public void FileSecurityConstants_AllowedMimeTypes_Should_Contain_Expected_Values()
    {
        // Assert
        Assert.Contains("image/jpeg", FileSecurityConstants.AllowedMimeTypes);
        Assert.Contains("image/png", FileSecurityConstants.AllowedMimeTypes);
        Assert.Contains("application/pdf", FileSecurityConstants.AllowedMimeTypes);
        Assert.Contains("text/plain", FileSecurityConstants.AllowedMimeTypes);
        Assert.Contains("application/zip", FileSecurityConstants.AllowedMimeTypes);
        Assert.Contains("video/mp4", FileSecurityConstants.AllowedMimeTypes);
        
        // Ensure it's immutable
        Assert.IsAssignableFrom<ImmutableHashSet<string>>(FileSecurityConstants.AllowedMimeTypes);
    }

    [Fact]
    public void FileSecurityConstants_DangerousExtensions_Should_Contain_Expected_Values()
    {
        // Assert
        Assert.Contains(".exe", FileSecurityConstants.DangerousExtensions);
        Assert.Contains(".bat", FileSecurityConstants.DangerousExtensions);
        Assert.Contains(".cmd", FileSecurityConstants.DangerousExtensions);
        Assert.Contains(".js", FileSecurityConstants.DangerousExtensions);
        Assert.Contains(".jar", FileSecurityConstants.DangerousExtensions);
        
        // Ensure it's immutable
        Assert.IsAssignableFrom<ImmutableHashSet<string>>(FileSecurityConstants.DangerousExtensions);
    }

    [Fact]
    public void FileSecurityConstants_DangerousMimeTypes_Should_Contain_Expected_Values()
    {
        // Assert
        Assert.Contains("application/x-executable", FileSecurityConstants.DangerousMimeTypes);
        Assert.Contains("application/x-msdownload", FileSecurityConstants.DangerousMimeTypes);
        Assert.Contains("text/javascript", FileSecurityConstants.DangerousMimeTypes);
        Assert.Contains("application/javascript", FileSecurityConstants.DangerousMimeTypes);
        
        // Ensure it's immutable
        Assert.IsAssignableFrom<ImmutableHashSet<string>>(FileSecurityConstants.DangerousMimeTypes);
    }

    [Fact]
    public void FileSecurityConstants_DangerousFileNameChars_Should_Contain_Expected_Values()
    {
        // Assert
        Assert.Contains('<', FileSecurityConstants.DangerousFileNameChars);
        Assert.Contains('>', FileSecurityConstants.DangerousFileNameChars);
        Assert.Contains(':', FileSecurityConstants.DangerousFileNameChars);
        Assert.Contains('"', FileSecurityConstants.DangerousFileNameChars);
        Assert.Contains('|', FileSecurityConstants.DangerousFileNameChars);
        Assert.Contains('?', FileSecurityConstants.DangerousFileNameChars);
        Assert.Contains('*', FileSecurityConstants.DangerousFileNameChars);
        Assert.Contains('\\', FileSecurityConstants.DangerousFileNameChars);
        Assert.Contains('/', FileSecurityConstants.DangerousFileNameChars);
        Assert.Contains('\0', FileSecurityConstants.DangerousFileNameChars);
        
        // Ensure it's immutable
        Assert.IsAssignableFrom<ImmutableArray<char>>(FileSecurityConstants.DangerousFileNameChars);
    }

    [Fact]
    public void FileSecurityConstants_DangerousPathComponents_Should_Contain_Expected_Values()
    {
        // Assert
        Assert.Contains("..", FileSecurityConstants.DangerousPathComponents);
        Assert.Contains("~", FileSecurityConstants.DangerousPathComponents);
        Assert.Contains("etc", FileSecurityConstants.DangerousPathComponents);
        Assert.Contains("windows", FileSecurityConstants.DangerousPathComponents);
        Assert.Contains("system32", FileSecurityConstants.DangerousPathComponents);
        Assert.Contains("program files", FileSecurityConstants.DangerousPathComponents);
        
        // Ensure it's immutable
        Assert.IsAssignableFrom<ImmutableHashSet<string>>(FileSecurityConstants.DangerousPathComponents);
    }

    [Fact]
    public void FileSecurityConstants_FileSignatures_Should_Contain_Expected_Values()
    {
        // Assert
        Assert.True(FileSecurityConstants.FileSignatures.ContainsKey(".jpg"));
        Assert.True(FileSecurityConstants.FileSignatures.ContainsKey(".png"));
        Assert.True(FileSecurityConstants.FileSignatures.ContainsKey(".pdf"));
        Assert.True(FileSecurityConstants.FileSignatures.ContainsKey(".zip"));
        Assert.True(FileSecurityConstants.FileSignatures.ContainsKey(".mp4"));
        
        // Check specific signatures
        Assert.Equal(new byte[] { 0xFF, 0xD8, 0xFF }, FileSecurityConstants.FileSignatures[".jpg"]);
        Assert.Equal(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, FileSecurityConstants.FileSignatures[".png"]);
        Assert.Equal(new byte[] { 0x25, 0x50, 0x44, 0x46 }, FileSecurityConstants.FileSignatures[".pdf"]);
        
        // Ensure it's immutable
        Assert.IsAssignableFrom<ImmutableDictionary<string, byte[]>>(FileSecurityConstants.FileSignatures);
    }
}