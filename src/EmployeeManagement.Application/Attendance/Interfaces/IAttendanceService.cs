using EmployeeManagement.Application.Attendance.Models;
using EmployeeManagement.Application.Common.Models;
using EmployeeManagement.Application.Employees.Models;

namespace EmployeeManagement.Application.Attendance.Interfaces;

public interface IAttendanceService
{
    Task<PagedResult<AttendanceListModel>> GetPagedAsync(
        string? search,
        DateTime? dateFrom,
        DateTime? dateTo,
        string? status,
        int pageNumber,
        int pageSize);

    Task<AttendanceDetailsModel?> GetByIdAsync(int id);

    Task<int> CreateAsync(
        AttendanceCreateModel model);

    Task<bool> UpdateAsync(
        int id,
        AttendanceCreateModel model);

    Task<bool> DeleteAsync(int id);
}