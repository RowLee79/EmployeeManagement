namespace EmployeeManagement.Application.Employees.Reports;

public class EmployeeReportModel
{
    public int Id { get; set; }

    public string EmployeeNumber { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? DepartmentName { get; set; }

    public string? PositionName { get; set; }

    public string EmploymentType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public decimal BasicSalary { get; set; }

    public DateTime HireDate { get; set; }

    public DateTime? RegularizationDate { get; set; }

    public string? ProfileImage { get; set; }
}