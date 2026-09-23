using EmployeeManagement.Domain.Common;
using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Domain.Entities;

public class LeaveBalance : AuditableEntity
{
    public int EmployeeId { get; set; }

    public int Year { get; set; }

    public LeaveType LeaveType { get; set; }

    public decimal AllocatedDays { get; set; }

    public decimal UsedDays { get; set; }

    public decimal RemainingDays =>
        AllocatedDays - UsedDays;

    public Employee Employee { get; set; } = null!;
}