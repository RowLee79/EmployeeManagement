using EmployeeManagement.Domain.Common;

namespace EmployeeManagement.Domain.Entities;

public class PayrollPeriod : AuditableEntity
{
    public string PeriodCode { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime PayDate { get; set; }

    public string Status { get; set; } = "Open";

    public string? Description { get; set; }

    public ICollection<Payroll> Payrolls { get; set; }
        = new List<Payroll>();
}