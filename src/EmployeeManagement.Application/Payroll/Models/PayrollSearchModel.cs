namespace EmployeeManagement.Application.Payroll.Models;

public class PayrollSearchModel
{
    public string? Search { get; set; }

    public string? Status { get; set; }

    public DateTime? PayrollDateFrom { get; set; }

    public DateTime? PayrollDateTo { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string SortBy { get; set; } = "PayrollDate";

    public bool SortDescending { get; set; } = true;
}