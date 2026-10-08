using System.Text;
namespace EmployeeManagement.Web.Services;
public sealed class LocalFileStorageService(IWebHostEnvironment environment) : IFileStorageService
{
    private const long MaxSize = 5 * 1024 * 1024;
    private string UploadDirectory => Path.Combine(environment.WebRootPath, "uploads", "employees");
    public async Task<string> SaveEmployeeProfileImageAsync(IFormFile file)
    {
        if (file is null || file.Length == 0 || file.Length > MaxSize)
            throw new InvalidOperationException("Choose an image between 1 byte and 5 MB.");
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        await using var input = file.OpenReadStream();
        var header = new byte[12];
        var count = await input.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false);
        var valid = extension switch
        {
            ".jpg" or ".jpeg" => count >= 3 && header[0] == 0xff && header[1] == 0xd8 && header[2] == 0xff,
            ".png" => count >= 8 && header.AsSpan(0, 8).SequenceEqual(new byte[] {137,80,78,71,13,10,26,10}),
            ".webp" => count >= 12 && Encoding.ASCII.GetString(header, 0, 4) == "RIFF" && Encoding.ASCII.GetString(header, 8, 4) == "WEBP",
            _ => false
        };
        if (!valid) throw new InvalidOperationException("Choose a JPG, PNG or WEBP image with matching file content.");
        Directory.CreateDirectory(UploadDirectory);
        var name = $"{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(UploadDirectory, name);
        try
        {
            await using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
            await output.WriteAsync(header.AsMemory(0, count));
            var buffer = new byte[81920];
            long written = count;
            int size;
            while ((size = await input.ReadAsync(buffer)) > 0)
            {
                written += size;
                if (written > MaxSize) throw new InvalidOperationException("Image exceeds 5 MB.");
                await output.WriteAsync(buffer.AsMemory(0, size));
            }
        }
        catch
        {
            File.Delete(path);
            throw;
        }
        return $"/uploads/employees/{name}";
    }
    public Task DeleteAsync(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return Task.CompletedTask;
        const string prefix = "/uploads/employees/";
        if (!relativePath.StartsWith(prefix, StringComparison.Ordinal))
            throw new InvalidOperationException("Only employee upload files may be deleted.");
        var name = relativePath[prefix.Length..];
        if (string.IsNullOrWhiteSpace(name) || name.Contains('/') || name.Contains('\\') || name.Contains(".."))
            throw new InvalidOperationException("Invalid employee image path.");
        File.Delete(Path.Combine(UploadDirectory, name));
        return Task.CompletedTask;
    }
}
