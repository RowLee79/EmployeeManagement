namespace EmployeeManagement.Application.Positions.Models;

public class PositionLookup
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int DepartmentId { get; set; }
}