using EmployeeManagement.Application.Common.Interfaces;
using EmployeeManagement.Application.Common.Models;
using EmployeeManagement.Application.LeaveManagement.Interfaces;
using EmployeeManagement.Application.LeaveManagement.Models;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Services;

public class LeaveService : ILeaveService
{
    private readonly EmployeeDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public LeaveService(
        EmployeeDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    // =========================================================
    // GET PAGED
    // =========================================================

    public async Task<PagedResult<LeaveRequestListModel>> GetPagedAsync(
        string? search,
        LeaveType? leaveType,
        LeaveStatus? status,
        DateTime? dateFrom,
        DateTime? dateTo,
        int pageNumber,
        int pageSize)
    {
        var query = _context.LeaveRequests
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .Include(x => x.Employee)
            .AsQueryable();

        // -----------------------------------------------------
        // SEARCH
        // -----------------------------------------------------

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();

            query = query.Where(x =>
                x.Employee.EmployeeNumber.Contains(searchTerm) ||
                x.Employee.FirstName.Contains(searchTerm) ||
                x.Employee.LastName.Contains(searchTerm) ||
                (x.Employee.MiddleName != null &&
                 x.Employee.MiddleName.Contains(searchTerm)) ||
                (x.Employee.Email != null &&
                 x.Employee.Email.Contains(searchTerm)) ||
                (x.Reason != null &&
                 x.Reason.Contains(searchTerm)));
        }

        // -----------------------------------------------------
        // LEAVE TYPE
        // -----------------------------------------------------

        if (leaveType.HasValue)
        {
            query = query.Where(x =>
                x.LeaveType == leaveType.Value);
        }

        // -----------------------------------------------------
        // STATUS
        // -----------------------------------------------------

        if (status.HasValue)
        {
            query = query.Where(x =>
                x.Status == status.Value);
        }

        // -----------------------------------------------------
        // DATE FROM
        // -----------------------------------------------------

        if (dateFrom.HasValue)
        {
            var fromDate = dateFrom.Value.Date;

            query = query.Where(x =>
                x.StartDate >= fromDate);
        }

        // -----------------------------------------------------
        // DATE TO
        // -----------------------------------------------------

        if (dateTo.HasValue)
        {
            var toDate =
                dateTo.Value.Date.AddDays(1);

            query = query.Where(x =>
                x.StartDate < toDate);
        }

        // -----------------------------------------------------
        // ORDERING
        // -----------------------------------------------------

        query = query
            .OrderByDescending(x => x.CreatedDate);

        // -----------------------------------------------------
        // PAGE SIZE
        // -----------------------------------------------------

        pageSize = pageSize switch
        {
            10 => 10,
            25 => 25,
            50 => 50,
            100 => 100,
            _ => 10
        };

        if (pageNumber < 1)
        {
            pageNumber = 1;
        }

        // -----------------------------------------------------
        // TOTAL COUNT
        // -----------------------------------------------------

        var totalCount =
            await query.CountAsync();

        var totalPages =
            (int)Math.Ceiling(
                totalCount / (double)pageSize);

        if (totalPages > 0 &&
            pageNumber > totalPages)
        {
            pageNumber = totalPages;
        }

        // -----------------------------------------------------
        // PAGINATION
        // -----------------------------------------------------

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new LeaveRequestListModel
            {
                Id = x.Id,

                EmployeeId =
                    x.EmployeeId,

                EmployeeNumber =
                    x.Employee.EmployeeNumber,

                EmployeeName =
                    x.Employee.FirstName +
                    " " +
                    x.Employee.LastName,

                LeaveType =
                    x.LeaveType,

                StartDate =
                    x.StartDate,

                EndDate =
                    x.EndDate,

                TotalDays =
                    x.TotalDays,

                Reason =
                    x.Reason,

                Status =
                    x.Status
            })
            .ToListAsync();

        return new PagedResult<LeaveRequestListModel>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    // =========================================================
    // GET BY ID
    // =========================================================

    public async Task<LeaveRequestDetailsModel?> GetByIdAsync(
        int id)
    {
        var request =
            await _context.LeaveRequests
                .AsNoTracking()
                .Include(x => x.Employee)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);

        if (request is null)
        {
            return null;
        }

        return new LeaveRequestDetailsModel
        {
            Id = request.Id,

            EmployeeId =
                request.EmployeeId,

            EmployeeNumber =
                request.Employee.EmployeeNumber,

            EmployeeName =
                request.Employee.FirstName +
                " " +
                request.Employee.LastName,

            LeaveType =
                request.LeaveType,

            StartDate =
                request.StartDate,

            EndDate =
                request.EndDate,

            TotalDays =
                request.TotalDays,

            Reason =
                request.Reason,

            Status =
                request.Status,

            ApprovedDate =
                request.ApprovedDate,

            ApprovedBy =
                request.ApprovedBy,

            RejectionReason =
                request.RejectionReason
        };
    }

    // =========================================================
    // CREATE
    // =========================================================

    public async Task<int> CreateAsync(
        LeaveRequestCreateModel model)
    {
        if (model.EndDate.Date < model.StartDate.Date)
        {
            throw new InvalidOperationException(
                "End date cannot be earlier than start date.");
        }

        var employeeExists =
            await _context.Employees.AnyAsync(x =>
                x.Id == model.EmployeeId &&
                !x.IsDeleted &&
                x.Status == EmployeeStatus.Active);

        if (!employeeExists)
        {
            throw new InvalidOperationException(
                "The selected employee does not exist or is inactive.");
        }

        var totalDays =
            CalculateLeaveDays(
                model.StartDate,
                model.EndDate);

        if (totalDays <= 0)
        {
            throw new InvalidOperationException(
                "The selected date range contains no working days.");
        }

        // -----------------------------------------------------
        // CHECK OVERLAPPING REQUESTS
        // -----------------------------------------------------

        var hasOverlap =
            await _context.LeaveRequests.AnyAsync(x =>
                x.EmployeeId == model.EmployeeId &&
                !x.IsDeleted &&
                x.Status != LeaveStatus.Rejected &&
                x.Status != LeaveStatus.Cancelled &&
                model.StartDate.Date <= x.EndDate.Date &&
                model.EndDate.Date >= x.StartDate.Date);

        if (hasOverlap)
        {
            throw new InvalidOperationException(
                "The employee already has a leave request that overlaps the selected dates.");
        }

        // -----------------------------------------------------
        // CHECK BALANCE
        // -----------------------------------------------------

        var year =
            model.StartDate.Year;

        var balance =
            await _context.LeaveBalances
                .FirstOrDefaultAsync(x =>
                    x.EmployeeId == model.EmployeeId &&
                    x.Year == year &&
                    x.LeaveType == model.LeaveType &&
                    !x.IsDeleted);

        if (balance is null)
        {
            throw new InvalidOperationException(
                "No leave balance is configured for the selected employee and leave type.");
        }

        if (balance.RemainingDays < totalDays)
        {
            throw new InvalidOperationException(
                $"Insufficient leave balance. Remaining balance: {balance.RemainingDays:0.##} day(s).");
        }

        // -----------------------------------------------------
        // CREATE
        // -----------------------------------------------------

        var currentUser =
            _currentUserService.Email ?? "System";

        var request = new LeaveRequest
        {
            EmployeeId =
                model.EmployeeId,

            LeaveType =
                model.LeaveType,

            StartDate =
                model.StartDate.Date,

            EndDate =
                model.EndDate.Date,

            TotalDays =
                totalDays,

            Reason =
                model.Reason,

            Status =
                LeaveStatus.Pending,

            CreatedDate =
                DateTime.UtcNow,

            CreatedBy =
                currentUser,

            IsDeleted =
                false
        };

        _context.LeaveRequests.Add(request);

        await _context.SaveChangesAsync();

        return request.Id;
    }

    // =========================================================
    // UPDATE
    // =========================================================

    public async Task<bool> UpdateAsync(
        int id,
        LeaveRequestCreateModel model)
    {
        var request =
            await _context.LeaveRequests
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);

        if (request is null)
        {
            return false;
        }

        if (request.Status != LeaveStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only pending leave requests can be updated.");
        }

        if (model.EndDate.Date < model.StartDate.Date)
        {
            throw new InvalidOperationException(
                "End date cannot be earlier than start date.");
        }

        var employeeExists =
            await _context.Employees.AnyAsync(x =>
                x.Id == model.EmployeeId &&
                !x.IsDeleted &&
                x.Status == EmployeeStatus.Active);

        if (!employeeExists)
        {
            throw new InvalidOperationException(
                "The selected employee does not exist or is inactive.");
        }

        var totalDays =
            CalculateLeaveDays(
                model.StartDate,
                model.EndDate);

        if (totalDays <= 0)
        {
            throw new InvalidOperationException(
                "The selected date range contains no working days.");
        }

        // -----------------------------------------------------
        // CHECK OVERLAPPING REQUESTS
        // -----------------------------------------------------

        var hasOverlap =
            await _context.LeaveRequests.AnyAsync(x =>
                x.Id != id &&
                x.EmployeeId == model.EmployeeId &&
                !x.IsDeleted &&
                x.Status != LeaveStatus.Rejected &&
                x.Status != LeaveStatus.Cancelled &&
                model.StartDate.Date <= x.EndDate.Date &&
                model.EndDate.Date >= x.StartDate.Date);

        if (hasOverlap)
        {
            throw new InvalidOperationException(
                "The employee already has a leave request that overlaps the selected dates.");
        }

        // -----------------------------------------------------
        // CHECK BALANCE
        // -----------------------------------------------------

        var balance =
            await _context.LeaveBalances
                .FirstOrDefaultAsync(x =>
                    x.EmployeeId == model.EmployeeId &&
                    x.Year == model.StartDate.Year &&
                    x.LeaveType == model.LeaveType &&
                    !x.IsDeleted);

        if (balance is null)
        {
            throw new InvalidOperationException(
                "No leave balance is configured for the selected employee and leave type.");
        }

        if (balance.RemainingDays < totalDays)
        {
            throw new InvalidOperationException(
                $"Insufficient leave balance. Remaining balance: {balance.RemainingDays:0.##} day(s).");
        }

        // -----------------------------------------------------
        // UPDATE
        // -----------------------------------------------------

        request.EmployeeId =
            model.EmployeeId;

        request.LeaveType =
            model.LeaveType;

        request.StartDate =
            model.StartDate.Date;

        request.EndDate =
            model.EndDate.Date;

        request.TotalDays =
            totalDays;

        request.Reason =
            model.Reason;

        request.UpdatedDate =
            DateTime.UtcNow;

        request.UpdatedBy =
            _currentUserService.Email ?? "System";

        await _context.SaveChangesAsync();

        return true;
    }

    // =========================================================
    // APPROVE
    // =========================================================

    public async Task<bool> ApproveAsync(
        int id,
        string approvedBy)
    {
        var request =
            await _context.LeaveRequests
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);

        if (request is null)
        {
            return false;
        }

        if (request.Status != LeaveStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only pending leave requests can be approved.");
        }

        // -----------------------------------------------------
        // GET BALANCE
        // -----------------------------------------------------

        var balance =
            await _context.LeaveBalances
                .FirstOrDefaultAsync(x =>
                    x.EmployeeId == request.EmployeeId &&
                    x.Year == request.StartDate.Year &&
                    x.LeaveType == request.LeaveType &&
                    !x.IsDeleted);

        if (balance is null)
        {
            throw new InvalidOperationException(
                "No leave balance is configured for this employee.");
        }

        // -----------------------------------------------------
        // CHECK BALANCE
        // -----------------------------------------------------

        if (balance.RemainingDays < request.TotalDays)
        {
            throw new InvalidOperationException(
                $"Insufficient leave balance. Remaining balance: {balance.RemainingDays:0.##} day(s).");
        }

        // -----------------------------------------------------
        // APPROVE
        // -----------------------------------------------------

        request.Status =
            LeaveStatus.Approved;

        request.ApprovedDate =
            DateTime.UtcNow;

        request.ApprovedBy =
            approvedBy;

        request.UpdatedDate =
            DateTime.UtcNow;

        request.UpdatedBy =
            approvedBy;

        // -----------------------------------------------------
        // DEDUCT BALANCE
        // -----------------------------------------------------

        balance.UsedDays +=
            request.TotalDays;

        balance.UpdatedDate =
            DateTime.UtcNow;

        balance.UpdatedBy =
            approvedBy;

        await _context.SaveChangesAsync();

        return true;
    }

    // =========================================================
    // REJECT
    // =========================================================

    public async Task<bool> RejectAsync(
        int id,
        string rejectedBy,
        string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException(
                "A rejection reason is required.");
        }

        var request =
            await _context.LeaveRequests
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);

        if (request is null)
        {
            return false;
        }

        if (request.Status != LeaveStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only pending leave requests can be rejected.");
        }

        request.Status =
            LeaveStatus.Rejected;

        request.RejectionReason =
            reason.Trim();

        request.UpdatedDate =
            DateTime.UtcNow;

        request.UpdatedBy =
            rejectedBy;

        await _context.SaveChangesAsync();

        return true;
    }

    // =========================================================
    // CANCEL
    // =========================================================

    public async Task<bool> CancelAsync(
        int id)
    {
        var request =
            await _context.LeaveRequests
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);

        if (request is null)
        {
            return false;
        }

        if (request.Status != LeaveStatus.Pending &&
            request.Status != LeaveStatus.Approved)
        {
            throw new InvalidOperationException(
                "Only pending or approved leave requests can be cancelled.");
        }

        var currentUser =
            _currentUserService.Email ?? "System";

        // -----------------------------------------------------
        // RESTORE BALANCE IF APPROVED
        // -----------------------------------------------------

        if (request.Status == LeaveStatus.Approved)
        {
            var balance =
                await _context.LeaveBalances
                    .FirstOrDefaultAsync(x =>
                        x.EmployeeId == request.EmployeeId &&
                        x.Year == request.StartDate.Year &&
                        x.LeaveType == request.LeaveType &&
                        !x.IsDeleted);

            if (balance is not null)
            {
                balance.UsedDays =
                    Math.Max(
                        0,
                        balance.UsedDays -
                        request.TotalDays);

                balance.UpdatedDate =
                    DateTime.UtcNow;

                balance.UpdatedBy =
                    currentUser;
            }
        }

        // -----------------------------------------------------
        // CANCEL
        // -----------------------------------------------------

        request.Status =
            LeaveStatus.Cancelled;

        request.UpdatedDate =
            DateTime.UtcNow;

        request.UpdatedBy =
            currentUser;

        await _context.SaveChangesAsync();

        return true;
    }

    // =========================================================
    // GET EMPLOYEE LEAVE BALANCES
    // =========================================================

    public async Task<IReadOnlyList<LeaveBalanceModel>>
        GetEmployeeBalancesAsync(
            int employeeId,
            int year)
    {
        return await _context.LeaveBalances
            .AsNoTracking()
            .Where(x =>
                x.EmployeeId == employeeId &&
                x.Year == year &&
                !x.IsDeleted)
            .OrderBy(x => x.LeaveType)
            .Select(x => new LeaveBalanceModel
            {
                Id = x.Id,

                EmployeeId =
                    x.EmployeeId,

                Year =
                    x.Year,

                LeaveType =
                    x.LeaveType,

                AllocatedDays =
                    x.AllocatedDays,

                UsedDays =
                    x.UsedDays,

                RemainingDays =
                    x.RemainingDays
            })
            .ToListAsync();
    }

    // =========================================================
    // CALCULATE LEAVE DAYS
    // =========================================================

    private static decimal CalculateLeaveDays(
        DateTime startDate,
        DateTime endDate)
    {
        var start =
            startDate.Date;

        var end =
            endDate.Date;

        if (end < start)
        {
            return 0;
        }

        decimal totalDays = 0;

        for (
            var date = start;
            date <= end;
            date = date.AddDays(1))
        {
            if (date.DayOfWeek != DayOfWeek.Saturday &&
                date.DayOfWeek != DayOfWeek.Sunday)
            {
                totalDays++;
            }
        }

        return totalDays;
    }
}