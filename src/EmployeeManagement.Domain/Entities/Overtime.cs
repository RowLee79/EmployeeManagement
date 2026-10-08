using EmployeeManagement.Domain.Common;

namespace EmployeeManagement.Domain.Entities;

public class Overtime : AuditableEntity
{
    public int EmployeeId { get; set; }

    public DateTime OvertimeDate { get; set; }

    public decimal Hours { get; set; }

    public decimal HourlyRate { get; set; }

    public decimal Multiplier { get; set; } = 1.25m;

    public decimal Amount { get; set; }

    public string Status { get; set; } = "Pending";

    public string? Remarks { get; set; }

    public Employee Employee { get; set; } = null!;
}