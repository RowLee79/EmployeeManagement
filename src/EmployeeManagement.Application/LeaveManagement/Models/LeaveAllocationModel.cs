using System.ComponentModel.DataAnnotations;
using EmployeeManagement.Domain.Enums;
namespace EmployeeManagement.Application.LeaveManagement.Models;
public class LeaveAllocationModel
{
    [Range(1, int.MaxValue)] public int EmployeeId { get; set; }
    [Range(2000, 2100)] public int Year { get; set; } = DateTime.Today.Year;
    [EnumDataType(typeof(LeaveType))] public LeaveType LeaveType { get; set; } = LeaveType.Vacation;
    [Range(typeof(decimal), "0", "366")] public decimal AllocatedDays { get; set; }
}
