namespace EmployeeManagement.Application.Departments.Models;

public class DepartmentListModel
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int EmployeeCount { get; set; }
    public int PositionCount { get; set; }
}