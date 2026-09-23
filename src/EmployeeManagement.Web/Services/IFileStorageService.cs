using Microsoft.AspNetCore.Http;

namespace EmployeeManagement.Web.Services;

public interface IFileStorageService
{
    Task<string> SaveEmployeeProfileImageAsync(
        IFormFile file);

    Task DeleteAsync(
        string? relativePath);
}