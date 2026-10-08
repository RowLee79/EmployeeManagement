namespace EmployeeManagement.Application.Payroll.Models;

public class PayrollCalculationInput
{
    public decimal BasicSalary { get; set; }

    public decimal Overtime { get; set; }

    public decimal Allowances { get; set; }

    public decimal OtherDeductions { get; set; }
}