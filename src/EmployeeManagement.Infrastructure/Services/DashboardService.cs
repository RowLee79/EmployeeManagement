using EmployeeManagement.Application.Dashboard.Interfaces;
using EmployeeManagement.Application.Dashboard.Models;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly EmployeeDbContext _context;

    public DashboardService(
        EmployeeDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardModel> GetDashboardAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var model = new DashboardModel
        {
            // ==================================================
            // Employees
            // ==================================================

            TotalEmployees =
                await _context.Employees
                    .CountAsync(x => !x.IsDeleted),

            ActiveEmployees =
                await _context.Employees
                    .CountAsync(x =>
                        !x.IsDeleted &&
                        x.Status == EmployeeStatus.Active),

            InactiveEmployees =
                await _context.Employees
                    .CountAsync(x =>
                        !x.IsDeleted &&
                        x.Status == EmployeeStatus.Inactive),

            OnLeaveEmployees =
                await _context.Employees
                    .CountAsync(x =>
                        !x.IsDeleted &&
                        x.Status == EmployeeStatus.OnLeave),


            // ==================================================
            // Organization
            // ==================================================

            TotalDepartments =
                await _context.Departments
                    .CountAsync(x =>
                        !x.IsDeleted &&
                        x.IsActive),

            TotalPositions =
                await _context.Positions
                    .CountAsync(x =>
                        !x.IsDeleted &&
                        x.IsActive),


            // ==================================================
            // Leave Requests
            // ==================================================

            PendingLeaveRequests =
                await _context.LeaveRequests
                    .CountAsync(x =>
                        !x.IsDeleted &&
                        x.Status == LeaveStatus.Pending),

            ApprovedLeaveRequests =
                await _context.LeaveRequests
                    .CountAsync(x =>
                        !x.IsDeleted &&
                        x.Status == LeaveStatus.Approved),

            RejectedLeaveRequests =
                await _context.LeaveRequests
                    .CountAsync(x =>
                        !x.IsDeleted &&
                        x.Status == LeaveStatus.Rejected)
        };


        // ======================================================
        // Today's Attendance
        // ======================================================

        var attendanceToday =
            await _context.Attendances
                .AsNoTracking()
               .Where(x =>
    !x.IsDeleted &&
    x.AttendanceDate >= today &&
    x.AttendanceDate < tomorrow)
                .ToListAsync();

        model.PresentToday =
            attendanceToday.Count(x =>
                x.Status == AttendanceStatus.Present);

        model.LateToday =
            attendanceToday.Count(x =>
                x.Status == AttendanceStatus.Late);

        model.AbsentToday =
            attendanceToday.Count(x =>
                x.Status == AttendanceStatus.Absent);

        model.HalfDayToday =
            attendanceToday.Count(x =>
                x.Status == AttendanceStatus.HalfDay);

        model.OnLeaveToday =
            attendanceToday.Count(x =>
                x.Status == AttendanceStatus.OnLeave);


        // ======================================================
        // Employees by Department
        // ======================================================

        model.EmployeesByDepartment =
            await _context.Employees
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted &&
                    x.Department != null)
                .GroupBy(x => x.Department!.Name)
                .Select(g => new DepartmentEmployeeCountModel
                {
                    DepartmentName = g.Key,
                    EmployeeCount = g.Count()
                })
                .OrderByDescending(x => x.EmployeeCount)
                .ToListAsync();


        // ======================================================
        // Recent Employees
        // ======================================================

        model.RecentEmployees =
            await _context.Employees
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedDate)
                .Take(5)
                .Select(x => new RecentEmployeeModel
                {
                    Id = x.Id,

                    EmployeeNumber =
                        x.EmployeeNumber,

                    FullName =
                        x.FirstName + " " + x.LastName,

                    DepartmentName =
                        x.Department != null
                            ? x.Department.Name
                            : null,

                    PositionName =
                        x.Position != null
                            ? x.Position.Name
                            : null,

                    HireDate =
                        x.HireDate,

                    Status =
                        x.Status.ToString()
                })
                .ToListAsync();

        return model;
    }
}