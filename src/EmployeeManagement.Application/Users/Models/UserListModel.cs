namespace EmployeeManagement.Application.Users.Models;

public class UserListModel
{
    public string Id { get; set; } = null!;

    public string? UserName { get; set; }

    public string? Email { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string FullName =>
        $"{FirstName} {LastName}".Trim();

    public bool IsActive { get; set; }

    public int? EmployeeId { get; set; }

    public string? EmployeeName { get; set; }

    public IReadOnlyList<string> Roles { get; set; }
        = Array.Empty<string>();
}