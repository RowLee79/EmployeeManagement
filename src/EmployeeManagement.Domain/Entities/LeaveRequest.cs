using EmployeeManagement.Domain.Common;

namespace EmployeeManagement.Domain.Entities;

public class LeaveRequest : AuditableEntity
{
    public int EmployeeId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal TotalDays { get; set; }

    public string Reason { get; set; } = null!;

    public string Status { get; set; } = "Pending";

    public Employee Employee { get; set; } = null!;
}