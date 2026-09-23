using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Application.Employees.Models;

public class EmployeeListModel
{
    public int Id { get; set; }

    public string EmployeeNumber { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public string LastName { get; set; } = null!;

    public string? Suffix { get; set; }

    public string FullName =>
        string.Join(
            " ",
            new[]
            {
            FirstName,
            MiddleName,
            LastName,
            Suffix
            }
            .Where(x => !string.IsNullOrWhiteSpace(x)));

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime HireDate { get; set; }

    public Gender Gender { get; set; }

    public EmploymentType EmploymentType { get; set; }

    public EmployeeStatus Status { get; set; }

    public decimal BasicSalary { get; set; }

    public int DepartmentId { get; set; }

    public string DepartmentName { get; set; } = null!;

    public int PositionId { get; set; }

    public string PositionName { get; set; } = null!;

    public string? ProfileImage { get; set; }
}