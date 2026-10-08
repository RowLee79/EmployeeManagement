using EmployeeManagement.Application.Payroll;
using EmployeeManagement.Application.Payroll.Interfaces;
using EmployeeManagement.Application.Payroll.Models;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Application.Payroll.Services;

public class PayrollPeriodService : IPayrollPeriodService
{
    private readonly EmployeeDbContext _context;

    public PayrollPeriodService(
        EmployeeDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PayrollPeriodListModel>>
        GetAllAsync()
    {
        var periods =
            await _context.PayrollPeriods
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.StartDate)
                .Select(x => new PayrollPeriodListModel
                {
                    Id = x.Id,
                    PeriodCode = x.PeriodCode,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    PayDate = x.PayDate,
                    Status = x.Status,
                    Description = x.Description,

                    PayrollCount =
                        x.Payrolls.Count(),

                    TotalGrossSalary =
                        x.Payrolls
                            .Select(p => (decimal?)p.GrossSalary)
                            .Sum() ?? 0m,

                    TotalDeductions =
                        x.Payrolls
                            .Select(p => (decimal?)p.Deductions)
                            .Sum() ?? 0m,

                    TotalNetSalary =
                        x.Payrolls
                            .Select(p => (decimal?)p.NetSalary)
                            .Sum() ?? 0m
                })
                .ToListAsync();

        return periods;
    }


    public async Task<PayrollPeriodDetailsModel?>
        GetByIdAsync(int id)
    {
        var period =
            await _context.PayrollPeriods
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    !x.IsDeleted)
                .Select(x => new PayrollPeriodDetailsModel
                {
                    Id = x.Id,
                    PeriodCode = x.PeriodCode,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    PayDate = x.PayDate,
                    Status = x.Status,
                    Description = x.Description,

                    PayrollCount =
                        x.Payrolls.Count(),

                    TotalGrossSalary =
                        x.Payrolls
                            .Select(p => (decimal?)p.GrossSalary)
                            .Sum() ?? 0m,

                    TotalDeductions =
                        x.Payrolls
                            .Select(p => (decimal?)p.Deductions)
                            .Sum() ?? 0m,

                    TotalNetSalary =
                        x.Payrolls
                            .Select(p => (decimal?)p.NetSalary)
                            .Sum() ?? 0m
                })
                .FirstOrDefaultAsync();

        return period;
    }


    public async Task<int> CreateAsync(
        PayrollPeriodCreateModel model)
    {
        Validate(model);

        var periodCode =
            model.PeriodCode.Trim();

        var exists =
            await _context.PayrollPeriods
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.PeriodCode == periodCode);

        if (exists)
        {
            throw new InvalidOperationException(
                $"Payroll period '{periodCode}' already exists.");
        }

        var period =
            new PayrollPeriod
            {
                PeriodCode = periodCode,
                StartDate = model.StartDate.Date,
                EndDate = model.EndDate.Date,
                PayDate = model.PayDate.Date,
                Description =
                    string.IsNullOrWhiteSpace(
                        model.Description)
                        ? null
                        : model.Description.Trim(),
                Status =
                    PayrollPeriodStatuses.Open
            };

        _context.PayrollPeriods.Add(period);

        await _context.SaveChangesAsync();

        return period.Id;
    }


    public async Task<bool> UpdateAsync(
        int id,
        PayrollPeriodCreateModel model)
    {
        Validate(model);

        var period =
            await _context.PayrollPeriods
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);

        if (period == null)
        {
            return false;
        }

        if (period.Status !=
            PayrollPeriodStatuses.Open)
        {
            throw new InvalidOperationException(
                "Only an Open payroll period can be edited.");
        }

        var periodCode =
            model.PeriodCode.Trim();

        var duplicate =
            await _context.PayrollPeriods
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.Id != id &&
                    x.PeriodCode == periodCode);

        if (duplicate)
        {
            throw new InvalidOperationException(
                $"Payroll period '{periodCode}' already exists.");
        }

        period.PeriodCode = periodCode;
        period.StartDate = model.StartDate.Date;
        period.EndDate = model.EndDate.Date;
        period.PayDate = model.PayDate.Date;

        period.Description =
            string.IsNullOrWhiteSpace(
                model.Description)
                ? null
                : model.Description.Trim();

        await _context.SaveChangesAsync();

        return true;
    }


    public async Task<bool> DeleteAsync(
        int id)
    {
        var period =
            await _context.PayrollPeriods
                .Include(x => x.Payrolls)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);

        if (period == null)
        {
            return false;
        }

        if (period.Status !=
            PayrollPeriodStatuses.Open)
        {
            throw new InvalidOperationException(
                "Only an Open payroll period can be deleted.");
        }

        if (period.Payrolls.Any())
        {
            throw new InvalidOperationException(
                "A payroll period with payroll records cannot be deleted.");
        }

        period.IsDeleted = true;

        await _context.SaveChangesAsync();

        return true;
    }


    public async Task<bool> StartProcessingAsync(
        int id)
    {
        var period =
            await GetPeriodForWorkflowAsync(id);

        if (period == null)
        {
            return false;
        }

        EnsureStatus(
            period,
            PayrollPeriodStatuses.Open);

        period.Status =
            PayrollPeriodStatuses.Processing;

        await _context.SaveChangesAsync();

        return true;
    }


    public async Task<bool> ApproveAsync(
        int id)
    {
        var period =
            await GetPeriodForWorkflowAsync(id);

        if (period == null)
        {
            return false;
        }

        EnsureStatus(
            period,
            PayrollPeriodStatuses.Processing);

        period.Status =
            PayrollPeriodStatuses.Approved;

        await _context.SaveChangesAsync();

        return true;
    }


    public async Task<bool> FinalizeAsync(
        int id)
    {
        var period =
            await GetPeriodForWorkflowAsync(id);

        if (period == null)
        {
            return false;
        }

        EnsureStatus(
            period,
            PayrollPeriodStatuses.Approved);

        period.Status =
            PayrollPeriodStatuses.Finalized;

        await _context.SaveChangesAsync();

        return true;
    }


    public async Task<bool> CloseAsync(
        int id)
    {
        var period =
            await GetPeriodForWorkflowAsync(id);

        if (period == null)
        {
            return false;
        }

        EnsureStatus(
            period,
            PayrollPeriodStatuses.Finalized);

        period.Status =
            PayrollPeriodStatuses.Closed;

        await _context.SaveChangesAsync();

        return true;
    }


    public async Task<bool> LockAsync(
        int id)
    {
        var period =
            await GetPeriodForWorkflowAsync(id);

        if (period == null)
        {
            return false;
        }

        EnsureStatus(
            period,
            PayrollPeriodStatuses.Closed);

        period.Status =
            PayrollPeriodStatuses.Locked;

        await _context.SaveChangesAsync();

        return true;
    }


    private async Task<PayrollPeriod?>
        GetPeriodForWorkflowAsync(int id)
    {
        return await _context.PayrollPeriods
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                !x.IsDeleted);
    }


    private static void EnsureStatus(
        PayrollPeriod period,
        string expectedStatus)
    {
        if (!string.Equals(
                period.Status,
                expectedStatus,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Payroll period must be in '{expectedStatus}' status. " +
                $"Current status is '{period.Status}'.");
        }
    }


    private static void Validate(
        PayrollPeriodCreateModel model)
    {
        if (model == null)
        {
            throw new ArgumentNullException(
                nameof(model));
        }

        if (string.IsNullOrWhiteSpace(
                model.PeriodCode))
        {
            throw new ArgumentException(
                "Period Code is required.",
                nameof(model));
        }

        if (model.StartDate == default)
        {
            throw new ArgumentException(
                "Start Date is required.",
                nameof(model));
        }

        if (model.EndDate == default)
        {
            throw new ArgumentException(
                "End Date is required.",
                nameof(model));
        }

        if (model.PayDate == default)
        {
            throw new ArgumentException(
                "Pay Date is required.",
                nameof(model));
        }

        if (model.EndDate.Date <
            model.StartDate.Date)
        {
            throw new ArgumentException(
                "End Date cannot be earlier than Start Date.",
                nameof(model));
        }

        if (model.PayDate.Date <
            model.EndDate.Date)
        {
            throw new ArgumentException(
                "Pay Date cannot be earlier than End Date.",
                nameof(model));
        }
    }

    public async Task<IReadOnlyList<PayrollPeriodLookup>>
    GetOpenLookupAsync()
    {
        var periods =
            await _context.PayrollPeriods
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted &&
                    x.Status == PayrollPeriodStatuses.Open)
                .OrderByDescending(x => x.StartDate)
                .Select(x => new PayrollPeriodLookup
                {
                    Id = x.Id,
                    PeriodCode = x.PeriodCode,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    PayDate = x.PayDate,
                    Status = x.Status
                })
                .ToListAsync();

        return periods;
    }
}