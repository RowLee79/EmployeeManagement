namespace EmployeeManagement.Application.Attendance.Models;

public class AttendanceListModel
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string EmployeeNumber { get; set; } = null!;

    public string EmployeeName { get; set; } = null!;

    public DateTime AttendanceDate { get; set; }

    public TimeSpan? TimeIn { get; set; }

    public TimeSpan? TimeOut { get; set; }

    public decimal RegularHours { get; set; }

    public decimal OvertimeHours { get; set; }

    public int LateMinutes { get; set; }

    public int UndertimeMinutes { get; set; }

    public string Status { get; set; } = null!;
}