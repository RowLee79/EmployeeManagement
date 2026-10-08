using EmployeeManagement.Application.LeaveManagement.Models;
namespace EmployeeManagement.Application.LeaveManagement.Interfaces;
public interface ILeaveAllocationService
{
    Task SetAllocationAsync(LeaveAllocationModel model);
}
