using EmployeeManagement.Domain.Common;

namespace EmployeeManagement.Domain.Entities;

public class Attendance : AuditableEntity
{
    public int EmployeeId { get; set; }

    public DateTime AttendanceDate { get; set; }

    public TimeSpan? TimeIn { get; set; }

    public TimeSpan? TimeOut { get; set; }

    public decimal RegularHours { get; set; }

    public decimal OvertimeHours { get; set; }

    public int LateMinutes { get; set; }

    public int UndertimeMinutes { get; set; }

    public string Status { get; set; } = "Present";

    public Employee Employee { get; set; } = null!;
}