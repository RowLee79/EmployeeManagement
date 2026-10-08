using System.ComponentModel.DataAnnotations;
using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Application.Attendance.Models;

public class AttendanceCreateModel
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime AttendanceDate { get; set; }

    public TimeSpan? TimeIn { get; set; }

    public TimeSpan? TimeOut { get; set; }

    public AttendanceStatus Status { get; set; } =
        AttendanceStatus.Present;
}