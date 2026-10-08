namespace EmployeeManagement.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }

    string? Email { get; }

    string? UserName { get; }

    bool IsAuthenticated { get; }

    IReadOnlyList<string> Roles { get; }

    bool IsInRole(string role);
}