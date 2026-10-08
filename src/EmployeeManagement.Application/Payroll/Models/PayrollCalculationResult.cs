namespace EmployeeManagement.Application.Payroll.Models;

public class PayrollCalculationResult
{
    public decimal BasicSalary { get; set; }

    public decimal Overtime { get; set; }

    public decimal Allowances { get; set; }

    public decimal GrossSalary { get; set; }

    public PayrollDeductionModel Deductions { get; set; } = new();

    public decimal NetSalary =>
        GrossSalary -
        Deductions.TotalDeductions;
}