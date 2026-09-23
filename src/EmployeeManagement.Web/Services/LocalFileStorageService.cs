using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace EmployeeManagement.Web.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;

    private static readonly string[] AllowedExtensions =
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private const long MaxFileSize =
        5 * 1024 * 1024;

    public LocalFileStorageService(
        IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveEmployeeProfileImageAsync(
        IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            throw new InvalidOperationException(
                "Please select an image.");
        }

        if (file.Length > MaxFileSize)
        {
            throw new InvalidOperationException(
                "Profile image cannot exceed 5 MB.");
        }

        var extension =
            Path.GetExtension(file.FileName)
                .ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                "Only JPG, JPEG, PNG and WEBP images are allowed.");
        }

        var uploadDirectory = Path.Combine(
            _environment.WebRootPath,
            "uploads",
            "employees");

        Directory.CreateDirectory(uploadDirectory);

        var fileName =
            $"{Guid.NewGuid():N}{extension}";

        var filePath = Path.Combine(
            uploadDirectory,
            fileName);

        await using var stream =
            new FileStream(
                filePath,
                FileMode.Create);

        await file.CopyToAsync(stream);

        return $"/uploads/employees/{fileName}";
    }

    public Task DeleteAsync(
        string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return Task.CompletedTask;

        var cleanPath =
            relativePath.TrimStart(
                '/',
                '\\');

        var filePath = Path.Combine(
            _environment.WebRootPath,
            cleanPath.Replace(
                '/',
                Path.DirectorySeparatorChar));

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }
}