using EmployeeManagement.Application.Departments.Models;

namespace EmployeeManagement.Application.Positions.Models;

public class PositionCreateModel
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int DepartmentId { get; set; }

    public decimal? MinimumSalary { get; set; }

    public decimal? MaximumSalary { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public IReadOnlyList<DepartmentLookup> Departments { get; set; }
        = [];
}