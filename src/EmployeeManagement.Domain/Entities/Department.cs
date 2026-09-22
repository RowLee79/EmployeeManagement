using EmployeeManagement.Domain.Common;

namespace EmployeeManagement.Domain.Entities;

public class Department : AuditableEntity
{
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Position> Positions { get; set; }
        = new List<Position>();

    public ICollection<Employee> Employees { get; set; }
        = new List<Employee>();
}