namespace EmployeeManagement.Application.Payroll.Models;

public class PayrollProcessingSummary
{
    public int PayrollPeriodId { get; set; }

    public string PayrollPeriodCode { get; set; }
        = string.Empty;

    public string Status { get; set; }
        = string.Empty;

    public int TotalEmployees { get; set; }

    public int TotalPayrolls { get; set; }

    public int DraftCount { get; set; }

    public int CalculatedCount { get; set; }

    public int ApprovedCount { get; set; }

    public int FinalizedCount { get; set; }

    public int CancelledCount { get; set; }

    public decimal TotalGrossSalary { get; set; }

    public decimal TotalDeductions { get; set; }

    public decimal TotalNetSalary { get; set; }

    public bool CanGenerate { get; set; }

    public bool CanCalculate { get; set; }

    public bool CanApprove { get; set; }

    public bool CanFinalize { get; set; }

    public bool CanClose { get; set; }

    public bool CanLock { get; set; }
}