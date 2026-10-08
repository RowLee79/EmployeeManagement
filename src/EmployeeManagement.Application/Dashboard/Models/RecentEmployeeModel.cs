namespace EmployeeManagement.Application.Dashboard.Models;

public class RecentEmployeeModel
{
    public int Id { get; set; }

    public string EmployeeNumber { get; set; }
        = string.Empty;

    public string FullName { get; set; }
        = string.Empty;

    public string? DepartmentName { get; set; }

    public string? PositionName { get; set; }

    public DateTime HireDate { get; set; }

    public string Status { get; set; }
        = string.Empty;
}