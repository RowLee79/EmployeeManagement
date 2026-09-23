namespace EmployeeManagement.Application.Employees.Models;

public class EmployeeLookupModel
{
    public int Id { get; set; }

    public string EmployeeNumber { get; set; } = null!;

    public string FullName { get; set; } = null!;
}