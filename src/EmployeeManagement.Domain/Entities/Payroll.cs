using EmployeeManagement.Domain.Common;

namespace EmployeeManagement.Domain.Entities;

public class Payroll : AuditableEntity
{
    public int EmployeeId { get; set; }

    public DateTime PayrollDate { get; set; }

    public decimal BasicSalary { get; set; }

    public decimal Overtime { get; set; }

    public decimal Allowances { get; set; }

    public decimal GrossSalary { get; set; }

    public decimal Deductions { get; set; }

    public decimal NetSalary { get; set; }

    public string Status { get; set; } = "Draft";

    public Employee Employee { get; set; } = null!;
}