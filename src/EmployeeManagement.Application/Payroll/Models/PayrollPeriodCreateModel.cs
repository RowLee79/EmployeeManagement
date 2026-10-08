namespace EmployeeManagement.Application.Payroll.Models;

public class PayrollPeriodCreateModel
{
    public string PeriodCode { get; set; }
        = string.Empty;

    public DateTime StartDate { get; set; }
        = DateTime.Today;

    public DateTime EndDate { get; set; }
        = DateTime.Today;

    public DateTime PayDate { get; set; }
        = DateTime.Today;

    public string? Description { get; set; }
}