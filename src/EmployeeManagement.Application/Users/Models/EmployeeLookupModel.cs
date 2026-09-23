namespace EmployeeManagement.Application.Users.Models;

public class EmployeeLookupModel
{
    public int Id { get; set; }

    public string EmployeeNumber { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;
}