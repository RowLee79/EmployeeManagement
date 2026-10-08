using EmployeeManagement.Application.Common.Models;
using EmployeeManagement.Application.LeaveManagement.Models;
using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Application.LeaveManagement.Interfaces;

public interface ILeaveService
{
    Task<PagedResult<LeaveRequestListModel>> GetPagedAsync(
        string? search,
        LeaveType? leaveType,
        LeaveStatus? status,
        DateTime? dateFrom,
        DateTime? dateTo,
        int pageNumber,
        int pageSize);

    Task<LeaveRequestDetailsModel?> GetByIdAsync(int id);

    Task<int> CreateAsync(
        LeaveRequestCreateModel model);

    Task<bool> UpdateAsync(
        int id,
        LeaveRequestCreateModel model);

    Task<bool> ApproveAsync(
        int id,
        string approvedBy);

    Task<bool> RejectAsync(
        int id,
        string rejectedBy,
        string reason);

    Task<bool> CancelAsync(int id);

    Task<IReadOnlyList<LeaveBalanceModel>>
        GetEmployeeBalancesAsync(
            int employeeId,
            int year);
}