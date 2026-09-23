namespace EmployeeManagement.Application.Users.Models;

public class UserDetailsModel
{
    public string Id { get; set; } = null!;

    public string? UserName { get; set; }

    public string? Email { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? PhoneNumber { get; set; }

    public bool EmailConfirmed { get; set; }

    public bool IsActive { get; set; }

    public int? EmployeeId { get; set; }

    public string? EmployeeName { get; set; }

    public IReadOnlyList<string> Roles { get; set; }
        = Array.Empty<string>();

    public DateTimeOffset? LockoutEnd { get; set; }

    public bool IsLockedOut { get; set; }
}