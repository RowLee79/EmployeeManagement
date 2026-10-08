namespace EmployeeManagement.Application.Positions.Models;

public class PositionListModel
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public decimal? MinimumSalary { get; set; }
    public decimal? MaximumSalary { get; set; }

    public bool IsActive { get; set; }

    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = null!;

    public int EmployeeCount { get; set; }
}