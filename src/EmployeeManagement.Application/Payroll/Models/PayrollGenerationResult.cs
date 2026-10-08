namespace EmployeeManagement.Application.Payroll.Models;

public class PayrollGenerationResult
{
    public int PayrollPeriodId { get; set; }

    public string PayrollPeriodCode { get; set; }
        = string.Empty;

    public int EmployeesProcessed { get; set; }

    public int PayrollsCreated { get; set; }

    public int PayrollsSkipped { get; set; }

    public decimal TotalGrossSalary { get; set; }

    public decimal TotalDeductions { get; set; }

    public decimal TotalNetSalary { get; set; }

    public IReadOnlyList<string> SkippedEmployees { get; set; }
        = [];
}