namespace EmployeeManagement.Application.Employees.Models;

public class EmployeeReportModel
{
    public int Id { get; set; }

    public string EmployeeNumber { get; set; } = null!;

    public string EmployeeName { get; set; } = null!;

    public string DepartmentName { get; set; } = null!;

    public string PositionName { get; set; } = null!;

    public string EmploymentType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime HireDate { get; set; }

    public decimal BasicSalary { get; set; }
}