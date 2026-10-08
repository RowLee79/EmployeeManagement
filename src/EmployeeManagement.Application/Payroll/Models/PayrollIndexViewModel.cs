using EmployeeManagement.Application.Common.Models;

namespace EmployeeManagement.Application.Payroll.Models;

public class PayrollIndexViewModel
{
    public PagedResult<PayrollListModel> Payrolls { get; set; }
        = null!;

    public string? Search { get; set; }

    public string? Status { get; set; }

    public DateTime? PayrollDateFrom { get; set; }

    public DateTime? PayrollDateTo { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public string SortBy { get; set; } = "PayrollDate";

    public bool SortDescending { get; set; } = true;

    public PayrollDashboardModel Dashboard { get; set; }
        = new();
}