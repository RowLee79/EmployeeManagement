using EmployeeManagement.Domain.Common;

namespace EmployeeManagement.Domain.Entities;

public class Position : AuditableEntity
{
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal? MinimumSalary { get; set; }

    public decimal? MaximumSalary { get; set; }

    public bool IsActive { get; set; } = true;

    public int DepartmentId { get; set; }

    public Department Department { get; set; } = null!;

    public ICollection<Employee> Employees { get; set; }
        = new List<Employee>();
}