namespace EmployeeManagement.Application.Payroll.Models;

public class PayslipModel
{
    public int PayrollId { get; set; }

    public int EmployeeId { get; set; }

    public string EmployeeNumber { get; set; } = string.Empty;

    public string EmployeeName { get; set; } = string.Empty;

    public string? DepartmentName { get; set; }

    public string? PositionName { get; set; }

    public DateTime PayrollDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal BasicSalary { get; set; }

    public decimal Overtime { get; set; }

    public decimal Allowances { get; set; }

    public decimal GrossSalary { get; set; }

    public PayrollDeductionModel Deductions { get; set; } = new();

    public decimal NetSalary { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? CreatedBy { get; set; }
}