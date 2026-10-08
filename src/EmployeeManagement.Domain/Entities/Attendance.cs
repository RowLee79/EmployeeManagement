using System.ComponentModel.DataAnnotations.Schema;
using EmployeeManagement.Domain.Enums;
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

    [NotMapped]
    public decimal LateHours => LateMinutes / 60m;

    [NotMapped]
    public decimal UndertimeHours => UndertimeMinutes / 60m;

    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

    [NotMapped]
    public string? Remarks { get; set; }

    public int LateMinutes { get; set; }

    public int UndertimeMinutes { get; set; }

    //public AttendanceStatus Status { get; set; }

    public Employee Employee { get; set; } = null!;
}