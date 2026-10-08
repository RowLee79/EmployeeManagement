namespace EmployeeManagement.Application.Payroll.Models;

public class PayrollPeriodLookup
{
    public int Id { get; set; }

    public string PeriodCode { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime PayDate { get; set; }

    public string Status { get; set; } = string.Empty;
}