using EmployeeManagement.Domain.Common;
using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Domain.Entities;

public class LeaveRequest : AuditableEntity
{
    public int EmployeeId { get; set; }

    public LeaveType LeaveType { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal TotalDays { get; set; }

    public string Reason { get; set; } = null!;

    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

    public DateTime? ApprovedDate { get; set; }

    public string? ApprovedBy { get; set; }

    public string? RejectionReason { get; set; }

    public Employee Employee { get; set; } = null!;
}