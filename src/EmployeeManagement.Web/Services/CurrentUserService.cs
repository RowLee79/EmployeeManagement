using System.Security.Claims;
using EmployeeManagement.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EmployeeManagement.Web.Services;

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


    // =========================================================
    // USER ID
    // =========================================================

    public string? UserId =>
        User?.FindFirstValue(ClaimTypes.NameIdentifier);


    // =========================================================
    // EMAIL
    // =========================================================

    public string? Email =>
        User?.FindFirstValue(ClaimTypes.Email)
        ?? User?.FindFirstValue("email");


    // =========================================================
    // USERNAME
    // =========================================================

    public string? UserName =>
        User?.FindFirstValue(ClaimTypes.Name)
        ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);


    // =========================================================
    // AUTHENTICATION
    // =========================================================

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated ?? false;


    // =========================================================
    // ROLES
    // =========================================================

    public IReadOnlyList<string> Roles
    {
        get
        {
            if (User == null)
            {
                return Array.Empty<string>();
            }

            return User
                .FindAll(ClaimTypes.Role)
                .Select(x => x.Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }


    // =========================================================
    // ROLE CHECK
    // =========================================================

    public bool IsInRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return false;
        }

        return User?.IsInRole(role) ?? false;
    }
}