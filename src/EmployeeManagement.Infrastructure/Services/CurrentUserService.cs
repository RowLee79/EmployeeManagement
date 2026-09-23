using System.Security.Claims;
using EmployeeManagement.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EmployeeManagement.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User =>
        _httpContextAccessor.HttpContext?.User;


    public string? UserId =>
        User?.FindFirstValue(
            ClaimTypes.NameIdentifier);


    public string? Email =>
        User?.FindFirstValue(
            ClaimTypes.Email)
        ?? User?.Identity?.Name;


    public string? UserName =>
        User?.Identity?.Name;


    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated == true;


    public IReadOnlyList<string> Roles
    {
        get
        {
            if (User is null)
                return Array.Empty<string>();

            return User.FindAll(ClaimTypes.Role)
                .Select(x => x.Value)
                .ToList();
        }
    }


    public bool IsInRole(string role)
    {
        return User?.IsInRole(role) == true;
    }
}