using EmployeeManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Application.LeaveManagement.Models;

public class LeaveRequestCreateModel
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public LeaveType LeaveType { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }
}