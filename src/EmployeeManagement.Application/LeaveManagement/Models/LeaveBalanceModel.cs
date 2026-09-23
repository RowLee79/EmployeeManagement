using EmployeeManagement.Domain.Enums;

public class LeaveBalanceModel
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int Year { get; set; }

    public LeaveType LeaveType { get; set; }

    public decimal AllocatedDays { get; set; }

    public decimal UsedDays { get; set; }

    public decimal RemainingDays { get; set; }
}