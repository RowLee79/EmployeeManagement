using EmployeeManagement.Application.Attendance.Interfaces;
using EmployeeManagement.Application.Attendance.Models;
using EmployeeManagement.Application.Common.Interfaces;
using EmployeeManagement.Application.Common.Models;
using EmployeeManagement.Application.Employees.Models;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Services;

public class AttendanceService : IAttendanceService
{
    private readonly EmployeeDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    private static readonly TimeSpan StandardStartTime =
        new(9, 0, 0);

    private const decimal StandardWorkHours = 8m;

    public AttendanceService(
        EmployeeDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;   
    }

    public async Task<PagedResult<AttendanceListModel>> GetPagedAsync(
        string? search,
        DateTime? dateFrom,
        DateTime? dateTo,
        string? status,
        int pageNumber,
        int pageSize)
    {
        var query = _context.Attendances
            .AsNoTracking()
            .Include(x => x.Employee)
            .Where(x =>
                !x.IsDeleted &&
                !x.Employee.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.Employee.EmployeeNumber.Contains(search) ||
                x.Employee.FirstName.Contains(search) ||
                x.Employee.LastName.Contains(search));
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(x =>
                x.AttendanceDate >= dateFrom.Value.Date);
        }

        if (dateTo.HasValue)
        {
            var endDate = dateTo.Value.Date.AddDays(1);

            query = query.Where(x =>
                x.AttendanceDate < endDate);
        }

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<AttendanceStatus>(
                status,
                true,
                out var parsedStatus))
        {
            query = query.Where(x =>
                x.Status == parsedStatus);
        }

        var totalCount =
            await query.CountAsync();

        var items =
            await query
                .OrderByDescending(x => x.AttendanceDate)
                .ThenBy(x => x.Employee.LastName)
                .ThenBy(x => x.Employee.FirstName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new AttendanceListModel
                {
                    Id = x.Id,

                    EmployeeId =
                        x.EmployeeId,

                    EmployeeNumber =
                        x.Employee.EmployeeNumber,

                    EmployeeName =
                        x.Employee.FirstName + " " +
                        x.Employee.LastName,

                    AttendanceDate =
                        x.AttendanceDate,

                    TimeIn =
                        x.TimeIn,

                    TimeOut =
                        x.TimeOut,

                    RegularHours =
                        x.RegularHours,

                    OvertimeHours =
                        x.OvertimeHours,

                    LateMinutes =
                        x.LateMinutes,

                    UndertimeMinutes =
                        x.UndertimeMinutes,

                    Status =
                        x.Status.ToString()
                })
                .ToListAsync();

        return new PagedResult<AttendanceListModel>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<AttendanceDetailsModel?> GetByIdAsync(
        int id)
    {
        return await _context.Attendances
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                !x.IsDeleted)
            .Select(x => new AttendanceDetailsModel
            {
                Id = x.Id,

                EmployeeId =
                    x.EmployeeId,

                EmployeeNumber =
                    x.Employee.EmployeeNumber,

                EmployeeName =
                    x.Employee.FirstName + " " +
                    x.Employee.LastName,

                AttendanceDate =
                    x.AttendanceDate,

                TimeIn =
                    x.TimeIn,

                TimeOut =
                    x.TimeOut,

                RegularHours =
                    x.RegularHours,

                OvertimeHours =
                    x.OvertimeHours,

                LateMinutes =
                    x.LateMinutes,

                UndertimeMinutes =
                    x.UndertimeMinutes,

                Status =
                    x.Status.ToString()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<int> CreateAsync(
        AttendanceCreateModel model)
    {
        var employeeExists =
            await _context.Employees
                .AnyAsync(x =>
                    x.Id == model.EmployeeId &&
                    !x.IsDeleted);

        if (!employeeExists)
        {
            throw new InvalidOperationException(
                "Employee does not exist.");
        }

        var attendanceExists =
            await _context.Attendances
                .AnyAsync(x =>
                    x.EmployeeId == model.EmployeeId &&
                    x.AttendanceDate.Date ==
                    model.AttendanceDate.Date &&
                    !x.IsDeleted);

        if (attendanceExists)
        {
            throw new InvalidOperationException(
                "Attendance already exists for this employee and date.");
        }

        ValidateTimes(model);

        var calculation =
            CalculateAttendance(
                model.AttendanceDate,
                model.TimeIn,
                model.TimeOut,
                model.Status);

        var attendance = new Attendance
        {
            EmployeeId =
                model.EmployeeId,

            AttendanceDate =
                model.AttendanceDate.Date,

            TimeIn =
                model.TimeIn,

            TimeOut =
                model.TimeOut,

            RegularHours =
                calculation.RegularHours,

            OvertimeHours =
                calculation.OvertimeHours,

            LateMinutes =
                calculation.LateMinutes,

            UndertimeMinutes =
                calculation.UndertimeMinutes,

            Status =
                calculation.Status,

            CreatedDate =
                DateTime.UtcNow,

            CreatedBy = _currentUserService.Email ?? "System",

            IsDeleted = false
        };

        _context.Attendances.Add(attendance);

        await _context.SaveChangesAsync();

        return attendance.Id;
    }

    public async Task<bool> UpdateAsync(
        int id,
        AttendanceCreateModel model)
    {
        var attendance =
            await _context.Attendances
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);

        if (attendance is null)
            return false;

        var duplicate =
            await _context.Attendances
                .AnyAsync(x =>
                    x.Id != id &&
                    x.EmployeeId == model.EmployeeId &&
                    x.AttendanceDate.Date ==
                    model.AttendanceDate.Date &&
                    !x.IsDeleted);

        if (duplicate)
        {
            throw new InvalidOperationException(
                "Attendance already exists for this employee and date.");
        }

        ValidateTimes(model);

        var calculation =
            CalculateAttendance(
                model.AttendanceDate,
                model.TimeIn,
                model.TimeOut,
                model.Status);

        attendance.EmployeeId =
            model.EmployeeId;

        attendance.AttendanceDate =
            model.AttendanceDate.Date;

        attendance.TimeIn =
            model.TimeIn;

        attendance.TimeOut =
            model.TimeOut;

        attendance.RegularHours =
            calculation.RegularHours;

        attendance.OvertimeHours =
            calculation.OvertimeHours;

        attendance.LateMinutes =
            calculation.LateMinutes;

        attendance.UndertimeMinutes =
            calculation.UndertimeMinutes;

        attendance.Status =
            calculation.Status;

        attendance.UpdatedDate =
            DateTime.UtcNow;

        attendance.UpdatedBy = _currentUserService.Email ?? "System";

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var attendance =
            await _context.Attendances
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);

        if (attendance is null)
            return false;

        attendance.IsDeleted = true;
        attendance.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    private static void ValidateTimes(
        AttendanceCreateModel model)
    {
        if (model.TimeIn.HasValue &&
            model.TimeOut.HasValue &&
            model.TimeOut <= model.TimeIn)
        {
            throw new InvalidOperationException(
                "Time Out must be later than Time In.");
        }
    }

    private static AttendanceCalculation
        CalculateAttendance(
            DateTime date,
            TimeSpan? timeIn,
            TimeSpan? timeOut,
            AttendanceStatus requestedStatus)
    {
        if (requestedStatus == AttendanceStatus.Absent ||
            requestedStatus == AttendanceStatus.OnLeave ||
            requestedStatus == AttendanceStatus.RestDay)
        {
            return new AttendanceCalculation
            {
                RegularHours = 0,
                OvertimeHours = 0,
                LateMinutes = 0,
                UndertimeMinutes = 0,
                Status = requestedStatus
            };
        }

        if (!timeIn.HasValue ||
            !timeOut.HasValue)
        {
            return new AttendanceCalculation
            {
                RegularHours = 0,
                OvertimeHours = 0,
                LateMinutes = 0,
                UndertimeMinutes = 0,
                Status = requestedStatus
            };
        }

        var totalHours =
            (decimal)(
                timeOut.Value -
                timeIn.Value)
            .TotalHours;

        totalHours =
            Math.Max(0, totalHours);

        var regularHours =
            Math.Min(
                StandardWorkHours,
                totalHours);

        var overtimeHours =
            Math.Max(
                0,
                totalHours -
                StandardWorkHours);

        var lateMinutes =
            timeIn.Value > StandardStartTime
                ? (int)(
                    timeIn.Value -
                    StandardStartTime)
                    .TotalMinutes
                : 0;

        var expectedEndTime =
            StandardStartTime.Add(
                TimeSpan.FromHours(
                    (double)StandardWorkHours));

        var undertimeMinutes = 0;

        if (timeOut.Value < expectedEndTime)
        {
            undertimeMinutes =
                (int)(
                    expectedEndTime -
                    timeOut.Value)
                    .TotalMinutes;
        }

        var status =
            lateMinutes > 0
                ? AttendanceStatus.Late
                : AttendanceStatus.Present;

        if (regularHours > 0 &&
            regularHours < 4)
        {
            status =
                AttendanceStatus.HalfDay;
        }

        return new AttendanceCalculation
        {
            RegularHours =
                Math.Round(
                    regularHours,
                    2),

            OvertimeHours =
                Math.Round(
                    overtimeHours,
                    2),

            LateMinutes =
                lateMinutes,

            UndertimeMinutes =
                undertimeMinutes,

            Status =
                status
        };
    }

    private sealed class AttendanceCalculation
    {
        public decimal RegularHours { get; init; }

        public decimal OvertimeHours { get; init; }

        public int LateMinutes { get; init; }

        public int UndertimeMinutes { get; init; }

        public AttendanceStatus Status { get; init; }
    }
}