namespace EmployeeManagement.Application.Payroll.Models;

public class PayrollDetailsModel
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string EmployeeNumber { get; set; } = string.Empty;

    public string EmployeeName { get; set; } = string.Empty;

    public DateTime PayrollDate { get; set; }

    public decimal BasicSalary { get; set; }

    public decimal Overtime { get; set; }

    public decimal Allowances { get; set; }

    public decimal GrossSalary { get; set; }

    public decimal Deductions { get; set; }

    public decimal NetSalary { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? UpdatedBy { get; set; }
}