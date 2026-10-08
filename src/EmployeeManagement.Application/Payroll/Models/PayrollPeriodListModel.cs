namespace EmployeeManagement.Application.Payroll.Models;

public class PayrollPeriodListModel
{
    public int Id { get; set; }

    public string PeriodCode { get; set; }
        = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime PayDate { get; set; }

    public string Status { get; set; }
        = string.Empty;

    public string? Description { get; set; }

    public int PayrollCount { get; set; }

    public decimal TotalGrossSalary { get; set; }

    public decimal TotalDeductions { get; set; }

    public decimal TotalNetSalary { get; set; }
}