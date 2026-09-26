namespace EmployeeManagement.Application.Payroll.Models;

public class PayrollDashboardModel
{
    public int TotalPayrolls { get; set; }

    public int DraftCount { get; set; }

    public int CalculatedCount { get; set; }

    public int ApprovedCount { get; set; }

    public int FinalizedCount { get; set; }

    public int CancelledCount { get; set; }

    public decimal TotalGrossSalary { get; set; }

    public decimal TotalDeductions { get; set; }

    public decimal TotalNetSalary { get; set; }
}