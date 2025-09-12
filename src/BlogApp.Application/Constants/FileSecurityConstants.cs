using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace BlogApp.Application.Constants;

[ExcludeFromCodeCoverage]
public static class FileSecurityConstants
{
    // Dosya boyutu limitleri (byte)
    public const long MaxFileSize = 100 * 1024 * 1024; // 100 MB
    public const long MinFileSize = 1; // 1 byte

    // Dosya adı güvenlik kuralları
    public const int MaxFileNameLength = 255;
    public const int MinFileNameLength = 1;

    // Presigned URL geçerlilik süresi (dakika)
    public const int PresignedUrlExpiryMinutes = 15;

    // Rate limiting için sabitler
    public const int MaxUploadRequestsPerHour = 100;
    public const int MaxUploadRequestsPerDay = 1000;

    // Regex güvenlik sabitleri (ReDoS koruması)
    public static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(100);
    public static readonly RegexOptions DefaultRegexOptions = RegexOptions.IgnoreCase | RegexOptions.Compiled;

    // İzin verilen dosya uzantıları
    public static readonly ImmutableHashSet<string> AllowedExtensions = ImmutableHashSet.Create(
        StringComparer.OrdinalIgnoreCase,
        // Resimler
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg",
        // Dokümanlar
        ".pdf", ".doc", ".docx", ".txt", ".rtf",
        // Arşivler
        ".zip", ".rar", ".7z",
        // Video
        ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm",
        // Ses
        ".mp3", ".wav", ".ogg", ".aac", ".flac"
    );

    // İzin verilen MIME türleri
    public static readonly ImmutableHashSet<string> AllowedMimeTypes = ImmutableHashSet.Create(
        StringComparer.OrdinalIgnoreCase,
        // Resimler
        "image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp", "image/svg+xml",
        // Dokümanlar
        "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "text/plain", "application/rtf",
        // Arşivler
        "application/zip", "application/x-rar-compressed", "application/x-7z-compressed",
        // Video
        "video/mp4", "video/avi", "video/quicktime", "video/x-ms-wmv", "video/x-flv", "video/webm",
        // Ses
        "audio/mpeg", "audio/wav", "audio/ogg", "audio/aac", "audio/flac"
    );

    // Tehlikeli dosya uzantıları (yasaklı)
    public static readonly ImmutableHashSet<string> DangerousExtensions = ImmutableHashSet.Create(
        StringComparer.OrdinalIgnoreCase,
        ".exe", ".bat", ".cmd", ".com", ".pif", ".scr", ".vbs", ".js", ".jar", ".msi",
        ".dll", ".sys", ".drv", ".ocx", ".cpl", ".hta", ".wsf", ".wsh", ".ps1", ".psm1",
        ".psd1", ".psc1", ".mst", ".msc", ".reg", ".inf", ".ini", ".log", ".tmp", ".temp"
    );

    // Tehlikeli MIME türleri (yasaklı)
    public static readonly ImmutableHashSet<string> DangerousMimeTypes = ImmutableHashSet.Create(
        StringComparer.OrdinalIgnoreCase,
        "application/x-executable", "application/x-msdownload", "application/x-msi",
        "application/x-msdos-program", "application/x-msdos-windows", "application/x-msi",
        "application/x-ms-shortcut", "application/x-ms-wim", "application/x-ms-wim",
        "text/javascript", "application/javascript", "application/x-javascript",
        "application/x-ms-manifest", "application/x-ms-application"
    );

    // Tehlikeli karakterler (dosya adında olmamalı)
    public static readonly ImmutableArray<char> DangerousFileNameChars = ImmutableArray.Create(
        '<', '>', ':', '"', '|', '?', '*', '\\', '/', '\0', '\t', '\n', '\r'
    );

    // Path traversal koruması için tehlikeli dizin isimleri
    public static readonly ImmutableHashSet<string> DangerousPathComponents = ImmutableHashSet.Create(
        StringComparer.OrdinalIgnoreCase,
        "..", "~", "etc", "proc", "sys", "dev", "bin", "sbin", "usr", "var", "tmp", "temp",
        "windows", "system32", "program files", "programdata", "appdata", "local"
    );

    // Dosya içeriği kontrolü için magic number'lar
    public static readonly ImmutableDictionary<string, byte[]> FileSignatures = ImmutableDictionary.CreateRange(
        new Dictionary<string, byte[]>
        {
            { ".jpg", new byte[] { 0xFF, 0xD8, 0xFF } },
            { ".jpeg", new byte[] { 0xFF, 0xD8, 0xFF } },
            { ".png", new byte[] { 0x89, 0x50, 0x4E, 0x47 } },
            { ".gif", new byte[] { 0x47, 0x49, 0x46 } },
            { ".pdf", new byte[] { 0x25, 0x50, 0x44, 0x46 } },
            { ".zip", new byte[] { 0x50, 0x4B, 0x03, 0x04 } },
            { ".mp4", new byte[] { 0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70 } }
        }
    );
}