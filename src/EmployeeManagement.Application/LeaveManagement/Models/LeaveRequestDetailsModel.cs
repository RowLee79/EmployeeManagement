using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Application.LeaveManagement.Models;

public class LeaveRequestDetailsModel
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string EmployeeNumber { get; set; } = null!;

    public string EmployeeName { get; set; } = null!;

    public LeaveType LeaveType { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal TotalDays { get; set; }

    public string? Reason { get; set; }

    public LeaveStatus Status { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public string? ApprovedBy { get; set; }

    public string? RejectionReason { get; set; }
}