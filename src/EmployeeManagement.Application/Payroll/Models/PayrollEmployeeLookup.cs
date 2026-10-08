namespace EmployeeManagement.Application.Payroll.Models;

public class PayrollEmployeeLookup
{
    public int Id { get; set; }

    public string EmployeeNumber { get; set; } = string.Empty;

    public string EmployeeName { get; set; } = string.Empty;

    public decimal BasicSalary { get; set; }
}