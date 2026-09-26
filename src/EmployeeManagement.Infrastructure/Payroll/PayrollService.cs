using EmployeeManagement.Application.Payroll;
using EmployeeManagement.Application.Payroll.Models;
using EmployeeManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using PayrollEntity = EmployeeManagement.Domain.Entities.Payroll;

namespace EmployeeManagement.Infrastructure.Payroll;

public class PayrollService : IPayrollService
{
    private readonly EmployeeDbContext _context;

    public PayrollService(EmployeeDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PayrollListModel>> GetAllAsync()
    {
        return await _context.Payrolls
            .AsNoTracking()
            .Include(x => x.Employee)
            .OrderByDescending(x => x.PayrollDate)
            .ThenBy(x => x.Employee.EmployeeNumber)
            .Select(x => new PayrollListModel
            {
                Id = x.Id,

                EmployeeId = x.EmployeeId,

                EmployeeNumber =
                    x.Employee.EmployeeNumber,

                EmployeeName =
                    x.Employee.FirstName + " " +
                    x.Employee.LastName,

                PayrollDate = x.PayrollDate,

                BasicSalary = x.BasicSalary,

                Overtime = x.Overtime,

                Allowances = x.Allowances,

                GrossSalary = x.GrossSalary,

                Deductions = x.Deductions,

                NetSalary = x.NetSalary,

                Status = x.Status
            })
            .ToListAsync();
    }

    public async Task<PayrollDetailsModel?> GetByIdAsync(int id)
    {
        return await _context.Payrolls
            .AsNoTracking()
            .Include(x => x.Employee)
            .Where(x => x.Id == id)
            .Select(x => new PayrollDetailsModel
            {
                Id = x.Id,

                EmployeeId = x.EmployeeId,

                EmployeeNumber =
                    x.Employee.EmployeeNumber,

                EmployeeName =
                    x.Employee.FirstName + " " +
                    x.Employee.LastName,

                PayrollDate = x.PayrollDate,

                BasicSalary = x.BasicSalary,

                Overtime = x.Overtime,

                Allowances = x.Allowances,

                GrossSalary = x.GrossSalary,

                Deductions = x.Deductions,

                NetSalary = x.NetSalary,

                Status = x.Status,

                CreatedDate = x.CreatedDate,

                CreatedBy = x.CreatedBy,

                UpdatedDate = x.UpdatedDate,

                UpdatedBy = x.UpdatedBy
            })
            .FirstOrDefaultAsync();
    }

    public async Task<int> CreateAsync(
        PayrollCreateModel model)
    {
        Validate(model);

        var employee =
            await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == model.EmployeeId &&
                    !x.IsDeleted);

        if (employee is null)
        {
            throw new InvalidOperationException(
                "Employee was not found.");
        }

        var startDate =
            model.PayrollDate.Date;

        var endDate =
            startDate.AddDays(1);

        var exists =
            await _context.Payrolls
                .AnyAsync(x =>
                    x.EmployeeId == model.EmployeeId &&
                    x.PayrollDate >= startDate &&
                    x.PayrollDate < endDate &&
                    x.Status != PayrollStatuses.Cancelled);

        if (exists)
        {
            throw new InvalidOperationException(
                "A payroll record already exists for this employee and payroll date.");
        }



        var payroll = new Domain.Entities.Payroll
        {
            EmployeeId = model.EmployeeId,

            PayrollDate = model.PayrollDate,

            BasicSalary = model.BasicSalary,

            Overtime = model.Overtime,

            Allowances = model.Allowances,

            GrossSalary =
                CalculateGrossSalary(model),

            Deductions = model.Deductions,

            NetSalary =
                CalculateNetSalary(model),

            Status = PayrollStatuses.Draft
        };

        _context.Payrolls.Add(payroll);

        await _context.SaveChangesAsync();

        return payroll.Id;
    }

    public async Task<bool> UpdateAsync(
        int id,
        PayrollCreateModel model)
    {
        Validate(model);

        var payroll =
            await _context.Payrolls
                .FirstOrDefaultAsync(x => x.Id == id);

        if (payroll is null)
        {
            return false;
        }

        if (payroll.Status != PayrollStatuses.Draft)
        {
            throw new InvalidOperationException(
                "Only Draft payroll can be edited.");
        }

        var startDate =
    model.PayrollDate.Date;

        var endDate =
            startDate.AddDays(1);

        var duplicate =
            await _context.Payrolls
                .AnyAsync(x =>
                    x.Id != id &&
                    x.EmployeeId == model.EmployeeId &&
                    x.PayrollDate >= startDate &&
                    x.PayrollDate < endDate &&
                    x.Status != PayrollStatuses.Cancelled);

        if (duplicate)
        {
            throw new InvalidOperationException(
                "Another payroll record already exists for this employee and payroll date.");
        }



        var employeeExists =
            await _context.Employees
                .AnyAsync(x =>
                    x.Id == model.EmployeeId &&
                    !x.IsDeleted);

        if (!employeeExists)
        {
            throw new InvalidOperationException(
                "Employee was not found.");
        }

        payroll.EmployeeId =
            model.EmployeeId;

        payroll.PayrollDate =
            model.PayrollDate;

        payroll.BasicSalary =
            model.BasicSalary;

        payroll.Overtime =
            model.Overtime;

        payroll.Allowances =
            model.Allowances;

        payroll.GrossSalary =
            CalculateGrossSalary(model);

        payroll.Deductions =
            model.Deductions;

        payroll.NetSalary =
            CalculateNetSalary(model);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var payroll =
            await _context.Payrolls
                .FirstOrDefaultAsync(x => x.Id == id);

        if (payroll is null)
        {
            return false;
        }

        if (payroll.Status != PayrollStatuses.Draft)
        {
            throw new InvalidOperationException(
                "Only Draft payroll can be deleted.");
        }

        _context.Payrolls.Remove(payroll);

        await _context.SaveChangesAsync();

        return true;
    }

    private static decimal CalculateGrossSalary(
        PayrollCreateModel model)
    {
        return model.BasicSalary
             + model.Overtime
             + model.Allowances;
    }

    private static decimal CalculateNetSalary(
        PayrollCreateModel model)
    {
        var gross =
            CalculateGrossSalary(model);

        return gross - model.Deductions;
    }
    private static void Validate(
        PayrollCreateModel model)
    {
        if (model.EmployeeId <= 0)
        {
            throw new InvalidOperationException(
                "Employee is required.");
        }

        if (model.BasicSalary < 0)
        {
            throw new InvalidOperationException(
                "Basic salary cannot be negative.");
        }

        if (model.Overtime < 0)
        {
            throw new InvalidOperationException(
                "Overtime cannot be negative.");
        }

        if (model.Allowances < 0)
        {
            throw new InvalidOperationException(
                "Allowances cannot be negative.");
        }

        if (model.Deductions < 0)
        {
            throw new InvalidOperationException(
                "Deductions cannot be negative.");
        }

        if (model.PayrollDate == default)
        {
            throw new InvalidOperationException(
                "Payroll date is required.");
        }
    }

    public async Task<IReadOnlyList<PayrollEmployeeLookup>>
    GetEmployeeLookupAsync()
    {
        return await _context.Employees
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.EmployeeNumber)
            .Select(x => new PayrollEmployeeLookup
            {
                Id = x.Id,

                EmployeeNumber =
                    x.EmployeeNumber,

                EmployeeName =
                    x.FirstName + " " +
                    x.LastName,

                BasicSalary =
                    x.BasicSalary
            })
            .ToListAsync();
    }

    public async Task<PayrollDashboardModel> GetDashboardAsync()
    {
        var payrolls =
            await _context.Payrolls
                .AsNoTracking()
                .ToListAsync();

        return new PayrollDashboardModel
        {
            TotalPayrolls = payrolls.Count,

            DraftCount =
                payrolls.Count(x =>
                    x.Status == PayrollStatuses.Draft),

            CalculatedCount =
                payrolls.Count(x =>
                    x.Status == PayrollStatuses.Calculated),

            ApprovedCount =
                payrolls.Count(x =>
                    x.Status == PayrollStatuses.Approved),

            FinalizedCount =
                payrolls.Count(x =>
                    x.Status == PayrollStatuses.Finalized),

            CancelledCount =
                payrolls.Count(x =>
                    x.Status == PayrollStatuses.Cancelled),

            TotalGrossSalary =
                payrolls
                    .Where(x =>
                        x.Status != PayrollStatuses.Cancelled)
                    .Sum(x => x.GrossSalary),

            TotalDeductions =
                payrolls
                    .Where(x =>
                        x.Status != PayrollStatuses.Cancelled)
                    .Sum(x => x.Deductions),

            TotalNetSalary =
                payrolls
                    .Where(x =>
                        x.Status != PayrollStatuses.Cancelled)
                    .Sum(x => x.NetSalary)
        };
    }

    public async Task<bool> CalculateAsync(int id)
    {
        var payroll =
            await _context.Payrolls
                .FirstOrDefaultAsync(x => x.Id == id);

        if (payroll is null)
        {
            return false;
        }

        if (payroll.Status != PayrollStatuses.Draft)
        {
            throw new InvalidOperationException(
                "Only Draft payroll can be calculated.");
        }

        payroll.GrossSalary =
            payroll.BasicSalary
            + payroll.Overtime
            + payroll.Allowances;

        payroll.NetSalary =
            payroll.GrossSalary
            - payroll.Deductions;

        payroll.Status =
            PayrollStatuses.Calculated;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ApproveAsync(int id)
    {
        var payroll =
            await _context.Payrolls
                .FirstOrDefaultAsync(x => x.Id == id);

        if (payroll is null)
        {
            return false;
        }

        if (payroll.Status != PayrollStatuses.Calculated)
        {
            throw new InvalidOperationException(
                "Only Calculated payroll can be approved.");
        }

        payroll.Status =
            PayrollStatuses.Approved;

        await _context.SaveChangesAsync();

        return true;
    }
    public async Task<bool> FinalizeAsync(int id)
    {
        var payroll =
            await _context.Payrolls
                .FirstOrDefaultAsync(x => x.Id == id);

        if (payroll is null)
        {
            return false;
        }

        if (payroll.Status != PayrollStatuses.Approved)
        {
            throw new InvalidOperationException(
                "Only Approved payroll can be finalized.");
        }

        payroll.Status =
            PayrollStatuses.Finalized;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CancelAsync(int id)
    {
        var payroll =
            await _context.Payrolls
                .FirstOrDefaultAsync(x => x.Id == id);

        if (payroll is null)
        {
            return false;
        }

        if (payroll.Status == PayrollStatuses.Finalized)
        {
            throw new InvalidOperationException(
                "Finalized payroll cannot be cancelled.");
        }

        if (payroll.Status == PayrollStatuses.Cancelled)
        {
            throw new InvalidOperationException(
                "Payroll is already cancelled.");
        }

        payroll.Status =
            PayrollStatuses.Cancelled;

        await _context.SaveChangesAsync();

        return true;
    }
}