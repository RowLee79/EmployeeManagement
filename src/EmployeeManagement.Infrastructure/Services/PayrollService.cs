using EmployeeManagement.Application.Common.Models;
using EmployeeManagement.Application.Payroll;
using EmployeeManagement.Application.Payroll.Interfaces;
using EmployeeManagement.Application.Payroll.Models;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

using PayrollEntity =
    EmployeeManagement.Domain.Entities.Payroll;

using PayrollDeductionEntity =
    EmployeeManagement.Domain.Entities.PayrollDeduction;

using PayrollPeriodEntity =
    EmployeeManagement.Domain.Entities.PayrollPeriod;

namespace EmployeeManagement.Application.Payroll.Services;

public class PayrollService : IPayrollService
{
    private readonly EmployeeDbContext _context;

    private readonly IPayrollCalculationEngine
        _calculationEngine;

    public PayrollService(
        EmployeeDbContext context,
        IPayrollCalculationEngine calculationEngine)
    {
        _context = context;
        _calculationEngine =
            calculationEngine;
    }


    // =========================================================
    // GET PAGED PAYROLLS
    // =========================================================

    public async Task<PagedResult<PayrollListModel>>
        GetPagedAsync(
            PayrollSearchModel searchModel)
    {
        var query =
            _context.Payrolls
                .AsNoTracking()
                .Include(x => x.Employee)
                .Include(x => x.PayrollPeriod)
                .AsQueryable();


        // -----------------------------------------------------
        // SEARCH
        // -----------------------------------------------------

        if (!string.IsNullOrWhiteSpace(
            searchModel.Search))
        {
            var search =
                searchModel.Search.Trim();

            query = query.Where(x =>
                x.Employee.EmployeeNumber
                    .Contains(search) ||

                x.Employee.FirstName
                    .Contains(search) ||

                x.Employee.LastName
                    .Contains(search) ||

                x.Employee.Email
                    .Contains(search));
        }


        // -----------------------------------------------------
        // STATUS
        // -----------------------------------------------------

        if (!string.IsNullOrWhiteSpace(
            searchModel.Status))
        {
            query = query.Where(x =>
                x.Status ==
                searchModel.Status);
        }


        // -----------------------------------------------------
        // PAYROLL DATE FROM
        // -----------------------------------------------------

        if (searchModel.PayrollDateFrom
            .HasValue)
        {
            var from =
                searchModel.PayrollDateFrom
                    .Value.Date;

            query = query.Where(x =>
                x.PayrollDate >= from);
        }


        // -----------------------------------------------------
        // PAYROLL DATE TO
        // -----------------------------------------------------

        if (searchModel.PayrollDateTo
            .HasValue)
        {
            var to =
                searchModel.PayrollDateTo
                    .Value.Date
                    .AddDays(1);

            query = query.Where(x =>
                x.PayrollDate < to);
        }


        // -----------------------------------------------------
        // SORTING
        // -----------------------------------------------------

        query =
            ApplySorting(
                query,
                searchModel.SortBy,
                searchModel.SortDescending);


        // -----------------------------------------------------
        // COUNT
        // -----------------------------------------------------

        var totalCount =
            await query.CountAsync();


        // -----------------------------------------------------
        // PAGE
        // -----------------------------------------------------

        var pageNumber =
            searchModel.PageNumber <= 0
                ? 1
                : searchModel.PageNumber;

        var pageSize =
            searchModel.PageSize <= 0
                ? 10
                : searchModel.PageSize;


        var items =
            await query
                .Skip(
                    (pageNumber - 1) *
                    pageSize)
                .Take(pageSize)
                .Select(x =>
                    new PayrollListModel
                    {
                        Id =
                            x.Id,

                        EmployeeId =
                            x.EmployeeId,

                        EmployeeNumber =
                            x.Employee.EmployeeNumber,

                        EmployeeName =
                            x.Employee.FirstName +
                            " " +
                            x.Employee.LastName,

                        PayrollPeriodId =
                            x.PayrollPeriodId,

                        PayrollPeriodCode =
                            x.PayrollPeriod == null
                                ? null
                                : x.PayrollPeriod
                                    .PeriodCode,

                        PayrollDate =
                            x.PayrollDate,

                        BasicSalary =
                            x.BasicSalary,

                        Overtime =
                            x.Overtime,

                        Allowances =
                            x.Allowances,

                        GrossSalary =
                            x.GrossSalary,

                        Deductions =
                            x.Deductions,

                        NetSalary =
                            x.NetSalary,

                        Status =
                            x.Status
                    })
                .ToListAsync();


        return new PagedResult<PayrollListModel>
        {
            Items = items,

            PageNumber =
                pageNumber,

            PageSize =
                pageSize,

            TotalCount =
                totalCount
        };
    }


    // =========================================================
    // SORTING
    // =========================================================

    private static IQueryable<PayrollEntity>
        ApplySorting(
            IQueryable<PayrollEntity> query,
            string? sortBy,
            bool descending)
    {
        var field =
            sortBy?.Trim().ToLowerInvariant();

        return field switch
        {
            "employeenumber" =>
                descending
                    ? query.OrderByDescending(
                        x => x.Employee.EmployeeNumber)
                    : query.OrderBy(
                        x => x.Employee.EmployeeNumber),

            "employeename" =>
                descending
                    ? query.OrderByDescending(
                        x => x.Employee.LastName)
                    : query.OrderBy(
                        x => x.Employee.LastName),

            "payrolldate" =>
                descending
                    ? query.OrderByDescending(
                        x => x.PayrollDate)
                    : query.OrderBy(
                        x => x.PayrollDate),

            "basicsalary" =>
                descending
                    ? query.OrderByDescending(
                        x => x.BasicSalary)
                    : query.OrderBy(
                        x => x.BasicSalary),

            "grosssalary" =>
                descending
                    ? query.OrderByDescending(
                        x => x.GrossSalary)
                    : query.OrderBy(
                        x => x.GrossSalary),

            "deductions" =>
                descending
                    ? query.OrderByDescending(
                        x => x.Deductions)
                    : query.OrderBy(
                        x => x.Deductions),

            "netsalary" =>
                descending
                    ? query.OrderByDescending(
                        x => x.NetSalary)
                    : query.OrderBy(
                        x => x.NetSalary),

            "status" =>
                descending
                    ? query.OrderByDescending(
                        x => x.Status)
                    : query.OrderBy(
                        x => x.Status),

            "payrollperiod" =>
                descending
                    ? query.OrderByDescending(
                        x => x.PayrollPeriod!.PeriodCode)
                    : query.OrderBy(
                        x => x.PayrollPeriod!.PeriodCode),

            _ =>
                query.OrderByDescending(
                    x => x.PayrollDate)
        };
    }


    // =========================================================
    // GET BY ID
    // =========================================================

    public async Task<PayrollDetailsModel?>
        GetByIdAsync(int id)
    {
        return await _context.Payrolls
            .AsNoTracking()
            .Include(x => x.Employee)
            .Include(x => x.PayrollPeriod)
            .Where(x => x.Id == id)
            .Select(x =>
                new PayrollDetailsModel
                {
                    Id =
                        x.Id,

                    EmployeeId =
                        x.EmployeeId,

                    EmployeeNumber =
                        x.Employee.EmployeeNumber,

                    EmployeeName =
                        x.Employee.FirstName +
                        " " +
                        x.Employee.LastName,

                    PayrollPeriodId =
                        x.PayrollPeriodId,

                    PayrollPeriodCode =
                        x.PayrollPeriod == null
                            ? null
                            : x.PayrollPeriod
                                .PeriodCode,

                    PayrollDate =
                        x.PayrollDate,

                    BasicSalary =
                        x.BasicSalary,

                    Overtime =
                        x.Overtime,

                    Allowances =
                        x.Allowances,

                    GrossSalary =
                        x.GrossSalary,

                    Deductions =
                        x.Deductions,

                    NetSalary =
                        x.NetSalary,

                    Status =
                        x.Status,

                    CreatedDate =
                        x.CreatedDate,

                    CreatedBy =
                        x.CreatedBy,

                    UpdatedDate =
                        x.UpdatedDate,

                    UpdatedBy =
                        x.UpdatedBy
                })
            .FirstOrDefaultAsync();
    }


    // =========================================================
    // EMPLOYEE LOOKUP
    // =========================================================

    public async Task<
        IReadOnlyList<PayrollEmployeeLookup>>
        GetEmployeeLookupAsync()
    {
        return await _context.Employees
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                x.Status ==
                    EmployeeStatus.Active)
            .OrderBy(x =>
                x.EmployeeNumber)
            .Select(x =>
                new PayrollEmployeeLookup
                {
                    Id =
                        x.Id,

                    EmployeeNumber =
                        x.EmployeeNumber,

                    EmployeeName =
                        x.FirstName +
                        " " +
                        x.LastName,

                    BasicSalary =
                        x.BasicSalary
                })
            .ToListAsync();
    }


    // =========================================================
    // PAYROLL PERIOD LOOKUP
    // =========================================================

    public async Task<
        IReadOnlyList<PayrollPeriodLookup>>
        GetPayrollPeriodLookupAsync()
    {
        return await _context.PayrollPeriods
            .AsNoTracking()
            .Where(x =>
                x.Status ==
                    PayrollPeriodStatuses.Open ||

                x.Status ==
                    PayrollPeriodStatuses.Processing)
            .OrderByDescending(x =>
                x.StartDate)
            .Select(x =>
                new PayrollPeriodLookup
                {
                    Id =
                        x.Id,

                    PeriodCode =
                        x.PeriodCode,

                    StartDate =
                        x.StartDate,

                    EndDate =
                        x.EndDate,

                    PayDate =
                        x.PayDate,

                    Status =
                        x.Status
                })
            .ToListAsync();
    }


    // =========================================================
    // DASHBOARD
    // =========================================================

    public async Task<PayrollDashboardModel>
        GetDashboardAsync()
    {
        var payrolls =
            _context.Payrolls
                .AsNoTracking();

        return new PayrollDashboardModel
        {
            TotalPayrolls =
                await payrolls.CountAsync(),

            DraftCount =
                await payrolls.CountAsync(
                    x => x.Status ==
                        PayrollStatuses.Draft),

            CalculatedCount =
                await payrolls.CountAsync(
                    x => x.Status ==
                        PayrollStatuses.Calculated),

            ApprovedCount =
                await payrolls.CountAsync(
                    x => x.Status ==
                        PayrollStatuses.Approved),

            FinalizedCount =
                await payrolls.CountAsync(
                    x => x.Status ==
                        PayrollStatuses.Finalized),

            CancelledCount =
                await payrolls.CountAsync(
                    x => x.Status ==
                        PayrollStatuses.Cancelled),

            TotalGrossSalary =
                await payrolls.SumAsync(
                    x => (decimal?)x.GrossSalary)
                ?? 0m,

            TotalDeductions =
                await payrolls.SumAsync(
                    x => (decimal?)x.Deductions)
                ?? 0m,

            TotalNetSalary =
                await payrolls.SumAsync(
                    x => (decimal?)x.NetSalary)
                ?? 0m
        };
    }


    // =========================================================
    // CREATE
    // =========================================================

    public async Task<int>
        CreateAsync(
            PayrollCreateModel model)
    {
        ValidateCreateModel(model);

        await ValidatePayrollPeriodAsync(
            model.PayrollPeriodId,
            model.PayrollDate);

        var employee =
            await _context.Employees
                .FirstOrDefaultAsync(x =>
                    x.Id == model.EmployeeId &&
                    !x.IsDeleted);

        if (employee == null)
        {
            throw new InvalidOperationException(
                "Employee was not found.");
        }

        if (employee.Status !=
            EmployeeStatus.Active)
        {
            throw new InvalidOperationException(
                "Payroll can only be created for an active employee.");
        }


        // -----------------------------------------------------
        // PREVENT DUPLICATE PAYROLL
        // -----------------------------------------------------

        if (model.PayrollPeriodId.HasValue)
        {
            var exists =
                await _context.Payrolls
                    .AnyAsync(x =>
                        x.PayrollPeriodId ==
                            model.PayrollPeriodId &&
                        x.EmployeeId ==
                            model.EmployeeId);

            if (exists)
            {
                throw new InvalidOperationException(
                    "Payroll already exists for this employee in the selected payroll period.");
            }
        }


        // -----------------------------------------------------
        // CALCULATION ENGINE
        // -----------------------------------------------------

        var calculation =
            _calculationEngine.Calculate(
                new PayrollCalculationInput
                {
                    BasicSalary =
                        model.BasicSalary,

                    Overtime =
                        model.Overtime,

                    Allowances =
                        model.Allowances,

                    OtherDeductions =
                        model.OtherDeductions
                });


        // -----------------------------------------------------
        // CREATE PAYROLL
        // -----------------------------------------------------

        var payroll =
            new PayrollEntity
            {
                EmployeeId =
                    model.EmployeeId,

                PayrollPeriodId =
                    model.PayrollPeriodId,

                PayrollDate =
                    model.PayrollDate,

                BasicSalary =
                    calculation.BasicSalary,

                Overtime =
                    calculation.Overtime,

                Allowances =
                    calculation.Allowances,

                GrossSalary =
                    calculation.GrossSalary,

                Deductions =
                    calculation.Deductions
                        .TotalDeductions,

                NetSalary =
                    calculation.NetSalary,

                Status =
                    PayrollStatuses.Draft
            };


        // -----------------------------------------------------
        // CREATE DEDUCTION
        // -----------------------------------------------------

        payroll.PayrollDeduction =
            CreateDeductionEntity(
                payroll,
                calculation.Deductions);


        _context.Payrolls.Add(payroll);

        await _context.SaveChangesAsync();

        return payroll.Id;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    public async Task<bool>
        UpdateAsync(
            int id,
            PayrollCreateModel model)
    {
        ValidateCreateModel(model);

        var payroll =
            await _context.Payrolls
                .Include(x =>
                    x.PayrollDeduction)
                .FirstOrDefaultAsync(
                    x => x.Id == id);

        if (payroll == null)
        {
            return false;
        }

        if (payroll.Status !=
            PayrollStatuses.Draft)
        {
            throw new InvalidOperationException(
                "Only Draft payrolls can be edited.");
        }


        await ValidatePayrollPeriodAsync(
            model.PayrollPeriodId,
            model.PayrollDate);


        // -----------------------------------------------------
        // PREVENT DUPLICATE PAYROLL
        // -----------------------------------------------------

        if (model.PayrollPeriodId.HasValue)
        {
            var exists =
                await _context.Payrolls
                    .AnyAsync(x =>
                        x.Id != id &&
                        x.PayrollPeriodId ==
                            model.PayrollPeriodId &&
                        x.EmployeeId ==
                            model.EmployeeId);

            if (exists)
            {
                throw new InvalidOperationException(
                    "Payroll already exists for this employee in the selected payroll period.");
            }
        }


        // -----------------------------------------------------
        // VALIDATE EMPLOYEE
        // -----------------------------------------------------

        var employee =
            await _context.Employees
                .FirstOrDefaultAsync(x =>
                    x.Id == model.EmployeeId &&
                    !x.IsDeleted);

        if (employee == null)
        {
            throw new InvalidOperationException(
                "Employee was not found.");
        }

        if (employee.Status !=
            EmployeeStatus.Active)
        {
            throw new InvalidOperationException(
                "Payroll can only be created for an active employee.");
        }


        // -----------------------------------------------------
        // CALCULATION ENGINE
        // -----------------------------------------------------

        var calculation =
            _calculationEngine.Calculate(
                new PayrollCalculationInput
                {
                    BasicSalary =
                        model.BasicSalary,

                    Overtime =
                        model.Overtime,

                    Allowances =
                        model.Allowances,

                    OtherDeductions =
                        model.OtherDeductions
                });


        // -----------------------------------------------------
        // UPDATE PAYROLL
        // -----------------------------------------------------

        payroll.EmployeeId =
            model.EmployeeId;

        payroll.PayrollPeriodId =
            model.PayrollPeriodId;

        payroll.PayrollDate =
            model.PayrollDate;

        payroll.BasicSalary =
            calculation.BasicSalary;

        payroll.Overtime =
            calculation.Overtime;

        payroll.Allowances =
            calculation.Allowances;

        payroll.GrossSalary =
            calculation.GrossSalary;

        payroll.Deductions =
            calculation.Deductions
                .TotalDeductions;

        payroll.NetSalary =
            calculation.NetSalary;


        // -----------------------------------------------------
        // UPDATE DEDUCTION
        // -----------------------------------------------------

        if (payroll.PayrollDeduction == null)
        {
            payroll.PayrollDeduction =
                CreateDeductionEntity(
                    payroll,
                    calculation.Deductions);
        }
        else
        {
            UpdateDeductionEntity(
                payroll.PayrollDeduction,
                calculation.Deductions);
        }


        await _context.SaveChangesAsync();

        return true;
    }


    // =========================================================
    // DELETE
    // =========================================================

    public async Task<bool>
        DeleteAsync(int id)
    {
        var payroll =
            await _context.Payrolls
                .FirstOrDefaultAsync(
                    x => x.Id == id);

        if (payroll == null)
        {
            return false;
        }

        if (payroll.Status !=
            PayrollStatuses.Draft)
        {
            throw new InvalidOperationException(
                "Only Draft payrolls can be deleted.");
        }

        _context.Payrolls.Remove(payroll);

        await _context.SaveChangesAsync();

        return true;
    }


    // =========================================================
    // CALCULATE INDIVIDUAL PAYROLL
    // =========================================================

    public async Task<bool>
        CalculateAsync(int id)
    {
        var payroll =
            await _context.Payrolls
                .Include(x =>
                    x.PayrollDeduction)
                .Include(x =>
                    x.PayrollPeriod)
                .FirstOrDefaultAsync(
                    x => x.Id == id);

        if (payroll == null)
        {
            return false;
        }

        if (payroll.Status !=
            PayrollStatuses.Draft)
        {
            return false;
        }

        ValidatePeriodForProcessing(
            payroll.PayrollPeriod);


        // -----------------------------------------------------
        // PRESERVE MANUAL DEDUCTIONS
        // -----------------------------------------------------

        var otherDeductions =
            payroll.PayrollDeduction
                ?.OtherDeductions
            ?? 0m;


        // -----------------------------------------------------
        // CALCULATION ENGINE
        // -----------------------------------------------------

        var calculation =
            _calculationEngine.Calculate(
                new PayrollCalculationInput
                {
                    BasicSalary =
                        payroll.BasicSalary,

                    Overtime =
                        payroll.Overtime,

                    Allowances =
                        payroll.Allowances,

                    OtherDeductions =
                        otherDeductions
                });


        // -----------------------------------------------------
        // UPDATE PAYROLL
        // -----------------------------------------------------

        payroll.GrossSalary =
            calculation.GrossSalary;

        payroll.Deductions =
            calculation.Deductions
                .TotalDeductions;

        payroll.NetSalary =
            calculation.NetSalary;

        payroll.Status =
            PayrollStatuses.Calculated;


        // -----------------------------------------------------
        // UPDATE DEDUCTION
        // -----------------------------------------------------

        if (payroll.PayrollDeduction == null)
        {
            payroll.PayrollDeduction =
                CreateDeductionEntity(
                    payroll,
                    calculation.Deductions);
        }
        else
        {
            UpdateDeductionEntity(
                payroll.PayrollDeduction,
                calculation.Deductions);
        }


        await _context.SaveChangesAsync();

        return true;
    }


    // =========================================================
    // APPROVE INDIVIDUAL PAYROLL
    // =========================================================

    public async Task<bool>
        ApproveAsync(int id)
    {
        var payroll =
            await _context.Payrolls
                .Include(x =>
                    x.PayrollPeriod)
                .FirstOrDefaultAsync(
                    x => x.Id == id);

        if (payroll == null)
        {
            return false;
        }

        if (payroll.Status !=
            PayrollStatuses.Calculated)
        {
            return false;
        }

        if (payroll.PayrollPeriod != null &&
            payroll.PayrollPeriod.Status !=
                PayrollPeriodStatuses.Processing)
        {
            return false;
        }

        payroll.Status =
            PayrollStatuses.Approved;

        await _context.SaveChangesAsync();

        return true;
    }


    // =========================================================
    // FINALIZE INDIVIDUAL PAYROLL
    // =========================================================

    public async Task<bool>
        FinalizeAsync(int id)
    {
        var payroll =
            await _context.Payrolls
                .Include(x =>
                    x.PayrollPeriod)
                .FirstOrDefaultAsync(
                    x => x.Id == id);

        if (payroll == null)
        {
            return false;
        }

        if (payroll.Status !=
            PayrollStatuses.Approved)
        {
            return false;
        }

        if (payroll.PayrollPeriod != null &&
            payroll.PayrollPeriod.Status !=
                PayrollPeriodStatuses.Approved)
        {
            return false;
        }

        payroll.Status =
            PayrollStatuses.Finalized;

        await _context.SaveChangesAsync();

        return true;
    }


    // =========================================================
    // CANCEL
    // =========================================================

    public async Task<bool>
        CancelAsync(int id)
    {
        var payroll =
            await _context.Payrolls
                .Include(x =>
                    x.PayrollPeriod)
                .FirstOrDefaultAsync(
                    x => x.Id == id);

        if (payroll == null)
        {
            return false;
        }

        if (payroll.Status ==
                PayrollStatuses.Finalized ||
            payroll.Status ==
                PayrollStatuses.Cancelled)
        {
            return false;
        }

        if (payroll.PayrollPeriod != null &&
            payroll.PayrollPeriod.Status !=
                PayrollPeriodStatuses.Open &&
            payroll.PayrollPeriod.Status !=
                PayrollPeriodStatuses.Processing)
        {
            return false;
        }

        payroll.Status =
            PayrollStatuses.Cancelled;

        await _context.SaveChangesAsync();

        return true;
    }


    // =========================================================
    // EMPLOYEE HISTORY
    // =========================================================

    public async Task<
        IReadOnlyList<PayrollListModel>>
        GetEmployeeHistoryAsync(
            int employeeId)
    {
        return await _context.Payrolls
            .AsNoTracking()
            .Include(x => x.Employee)
            .Include(x => x.PayrollPeriod)
            .Where(x =>
                x.EmployeeId ==
                employeeId)
            .OrderByDescending(x =>
                x.PayrollDate)
            .Select(x =>
                new PayrollListModel
                {
                    Id =
                        x.Id,

                    EmployeeId =
                        x.EmployeeId,

                    EmployeeNumber =
                        x.Employee.EmployeeNumber,

                    EmployeeName =
                        x.Employee.FirstName +
                        " " +
                        x.Employee.LastName,

                    PayrollPeriodId =
                        x.PayrollPeriodId,

                    PayrollPeriodCode =
                        x.PayrollPeriod == null
                            ? null
                            : x.PayrollPeriod
                                .PeriodCode,

                    PayrollDate =
                        x.PayrollDate,

                    BasicSalary =
                        x.BasicSalary,

                    Overtime =
                        x.Overtime,

                    Allowances =
                        x.Allowances,

                    GrossSalary =
                        x.GrossSalary,

                    Deductions =
                        x.Deductions,

                    NetSalary =
                        x.NetSalary,

                    Status =
                        x.Status
                })
            .ToListAsync();
    }


    // =========================================================
    // PAYSLIP
    // =========================================================

    public async Task<PayslipModel?>
        GetPayslipAsync(
            int payrollId)
    {
        var payroll =
            await _context.Payrolls
                .AsNoTracking()
                .Include(x =>
                    x.Employee)
                    .ThenInclude(e =>
                        e.Department)
                .Include(x =>
                    x.Employee)
                    .ThenInclude(e =>
                        e.Position)
                .Include(x =>
                    x.PayrollDeduction)
                .FirstOrDefaultAsync(
                    x => x.Id == payrollId);

        if (payroll == null)
        {
            return null;
        }

        return new PayslipModel
        {
            PayrollId =
                payroll.Id,

            EmployeeId =
                payroll.EmployeeId,

            EmployeeNumber =
                payroll.Employee.EmployeeNumber,

            EmployeeName =
                payroll.Employee.FirstName +
                " " +
                payroll.Employee.LastName,

            DepartmentName =
                payroll.Employee.Department?.Name,

            PositionName =
                payroll.Employee.Position?.Name,

            PayrollDate =
                payroll.PayrollDate,

            Status =
                payroll.Status,

            BasicSalary =
                payroll.BasicSalary,

            Overtime =
                payroll.Overtime,

            Allowances =
                payroll.Allowances,

            GrossSalary =
                payroll.GrossSalary,

            Deductions =
                payroll.PayrollDeduction == null
                    ? new PayrollDeductionModel()
                    : new PayrollDeductionModel
                    {
                        Sss =
                            payroll.PayrollDeduction
                                .Sss,

                        PhilHealth =
                            payroll.PayrollDeduction
                                .PhilHealth,

                        PagIbig =
                            payroll.PayrollDeduction
                                .PagIbig,

                        WithholdingTax =
                            payroll.PayrollDeduction
                                .WithholdingTax,

                        OtherDeductions =
                            payroll.PayrollDeduction
                                .OtherDeductions
                    },

            NetSalary =
                payroll.NetSalary,

            CreatedDate =
                payroll.CreatedDate,

            CreatedBy =
                payroll.CreatedBy
        };
    }


    // =========================================================
    // PAYROLLS BY PERIOD
    // =========================================================

    public async Task<
        IReadOnlyList<PayrollListModel>>
        GetByPayrollPeriodAsync(
            int payrollPeriodId)
    {
        return await _context.Payrolls
            .AsNoTracking()
            .Include(x =>
                x.Employee)
            .Include(x =>
                x.PayrollPeriod)
            .Where(x =>
                x.PayrollPeriodId ==
                payrollPeriodId)
            .OrderBy(x =>
                x.Employee.EmployeeNumber)
            .Select(x =>
                new PayrollListModel
                {
                    Id =
                        x.Id,

                    EmployeeId =
                        x.EmployeeId,

                    EmployeeNumber =
                        x.Employee.EmployeeNumber,

                    EmployeeName =
                        x.Employee.FirstName +
                        " " +
                        x.Employee.LastName,

                    PayrollPeriodId =
                        x.PayrollPeriodId,

                    PayrollPeriodCode =
                        x.PayrollPeriod == null
                            ? null
                            : x.PayrollPeriod
                                .PeriodCode,

                    PayrollDate =
                        x.PayrollDate,

                    BasicSalary =
                        x.BasicSalary,

                    Overtime =
                        x.Overtime,

                    Allowances =
                        x.Allowances,

                    GrossSalary =
                        x.GrossSalary,

                    Deductions =
                        x.Deductions,

                    NetSalary =
                        x.NetSalary,

                    Status =
                        x.Status
                })
            .ToListAsync();
    }


    // =========================================================
    // GENERATE PAYROLL
    // =========================================================

    public async Task<PayrollGenerationResult>
        GeneratePayrollAsync(
            int payrollPeriodId)
    {
        var period =
            await _context.PayrollPeriods
                .FirstOrDefaultAsync(x =>
                    x.Id == payrollPeriodId &&
                    !x.IsDeleted);

        if (period == null)
        {
            throw new InvalidOperationException(
                "Payroll period was not found.");
        }

        if (period.Status !=
            PayrollPeriodStatuses.Open)
        {
            throw new InvalidOperationException(
                "Payroll can only be generated for an Open payroll period.");
        }


        // -----------------------------------------------------
        // GET ACTIVE EMPLOYEES
        // -----------------------------------------------------

        var employees =
            await _context.Employees
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted &&
                    x.Status ==
                        EmployeeStatus.Active)
                .OrderBy(x =>
                    x.EmployeeNumber)
                .ToListAsync();

        if (employees.Count == 0)
        {
            throw new InvalidOperationException(
                "No active employees were found.");
        }


        // -----------------------------------------------------
        // EXISTING PAYROLLS
        // -----------------------------------------------------

        var existingEmployeeIds =
            await _context.Payrolls
                .Where(x =>
                    !x.IsDeleted &&
                    x.PayrollPeriodId ==
                        payrollPeriodId)
                .Select(x =>
                    x.EmployeeId)
                .ToHashSetAsync();


        var result =
            new PayrollGenerationResult
            {
                PayrollPeriodId =
                    period.Id,

                PayrollPeriodCode =
                    period.PeriodCode,

                EmployeesProcessed =
                    employees.Count
            };


        // -----------------------------------------------------
        // GENERATE
        // -----------------------------------------------------

        foreach (var employee in employees)
        {
            if (existingEmployeeIds.Contains(
                employee.Id))
            {
                result.PayrollsSkipped++;

                result.SkippedEmployees =
                    result.SkippedEmployees
                        .Append(
                            $"{employee.EmployeeNumber} - " +
                            $"{employee.FirstName} " +
                            $"{employee.LastName}")
                        .ToList();

                continue;
            }


            // -------------------------------------------------
            // CALCULATION ENGINE
            // -------------------------------------------------

            var calculation =
                _calculationEngine.Calculate(
                    new PayrollCalculationInput
                    {
                        BasicSalary =
                            employee.BasicSalary,

                        Overtime =
                            0m,

                        Allowances =
                            0m,

                        OtherDeductions =
                            0m
                    });


            // -------------------------------------------------
            // CREATE PAYROLL
            // -------------------------------------------------

            var payroll =
                new PayrollEntity
                {
                    EmployeeId =
                        employee.Id,

                    PayrollPeriodId =
                        payrollPeriodId,

                    PayrollDate =
                        period.PayDate,

                    BasicSalary =
                        calculation.BasicSalary,

                    Overtime =
                        calculation.Overtime,

                    Allowances =
                        calculation.Allowances,

                    GrossSalary =
                        calculation.GrossSalary,

                    Deductions =
                        calculation.Deductions
                            .TotalDeductions,

                    NetSalary =
                        calculation.NetSalary,

                    Status =
                        PayrollStatuses.Draft
                };


            // -------------------------------------------------
            // CREATE DEDUCTION THROUGH NAVIGATION
            // -------------------------------------------------

            payroll.PayrollDeduction =
                CreateDeductionEntity(
                    payroll,
                    calculation.Deductions);


            _context.Payrolls.Add(payroll);

            await _context.SaveChangesAsync();


            // -------------------------------------------------
            // RESULT
            // -------------------------------------------------

            result.PayrollsCreated++;

            result.TotalGrossSalary +=
                payroll.GrossSalary;

            result.TotalDeductions +=
                payroll.Deductions;

            result.TotalNetSalary +=
                payroll.NetSalary;
        }


        return result;
    }


    // =========================================================
    // CALCULATE ALL
    // =========================================================

    public async Task<int>
        CalculateAllAsync(
            int payrollPeriodId)
    {
        var period =
            await _context.PayrollPeriods
                .FirstOrDefaultAsync(
                    x => x.Id ==
                        payrollPeriodId);

        if (period == null)
        {
            throw new InvalidOperationException(
                "Payroll period was not found.");
        }

        if (period.Status !=
            PayrollPeriodStatuses.Processing)
        {
            throw new InvalidOperationException(
                "Payroll period must be in Processing status.");
        }


        var payrolls =
            await _context.Payrolls
                .Include(x =>
                    x.PayrollDeduction)
                .Where(x =>
                    x.PayrollPeriodId ==
                        payrollPeriodId &&
                    x.Status ==
                        PayrollStatuses.Draft)
                .ToListAsync();


        var calculatedCount = 0;


        foreach (var payroll in payrolls)
        {
            // -------------------------------------------------
            // PRESERVE MANUAL DEDUCTIONS
            // -------------------------------------------------

            var otherDeductions =
                payroll.PayrollDeduction
                    ?.OtherDeductions
                ?? 0m;


            // -------------------------------------------------
            // CALCULATION ENGINE
            // -------------------------------------------------

            var calculation =
                _calculationEngine.Calculate(
                    new PayrollCalculationInput
                    {
                        BasicSalary =
                            payroll.BasicSalary,

                        Overtime =
                            payroll.Overtime,

                        Allowances =
                            payroll.Allowances,

                        OtherDeductions =
                            otherDeductions
                    });


            // -------------------------------------------------
            // UPDATE PAYROLL
            // -------------------------------------------------

            payroll.GrossSalary =
                calculation.GrossSalary;

            payroll.Deductions =
                calculation.Deductions
                    .TotalDeductions;

            payroll.NetSalary =
                calculation.NetSalary;


            // -------------------------------------------------
            // UPDATE DEDUCTION
            // -------------------------------------------------

            if (payroll.PayrollDeduction == null)
            {
                payroll.PayrollDeduction =
                    CreateDeductionEntity(
                        payroll,
                        calculation.Deductions);
            }
            else
            {
                UpdateDeductionEntity(
                    payroll.PayrollDeduction,
                    calculation.Deductions);
            }


            payroll.Status =
                PayrollStatuses.Calculated;

            calculatedCount++;
        }


        await _context.SaveChangesAsync();

        return calculatedCount;
    }


    // =========================================================
    // APPROVE ALL
    // =========================================================

    public async Task<int>
        ApproveAllAsync(
            int payrollPeriodId)
    {
        var period =
            await _context.PayrollPeriods
                .FirstOrDefaultAsync(
                    x => x.Id ==
                        payrollPeriodId);

        if (period == null)
        {
            throw new InvalidOperationException(
                "Payroll period was not found.");
        }

        if (period.Status !=
            PayrollPeriodStatuses.Processing)
        {
            throw new InvalidOperationException(
                "Payroll period must be in Processing status.");
        }


        // -----------------------------------------------------
        // DON'T ALLOW APPROVAL IF DRAFT EXISTS
        // -----------------------------------------------------

        var hasDraftPayroll =
            await _context.Payrolls
                .AnyAsync(x =>
                    x.PayrollPeriodId ==
                        payrollPeriodId &&
                    x.Status ==
                        PayrollStatuses.Draft);

        if (hasDraftPayroll)
        {
            throw new InvalidOperationException(
                "All payrolls must be calculated before approval.");
        }


        // -----------------------------------------------------
        // GET CALCULATED PAYROLLS
        // -----------------------------------------------------

        var payrolls =
            await _context.Payrolls
                .Where(x =>
                    x.PayrollPeriodId ==
                        payrollPeriodId &&
                    x.Status ==
                        PayrollStatuses.Calculated)
                .ToListAsync();


        if (payrolls.Count == 0)
        {
            throw new InvalidOperationException(
                "There are no Calculated payrolls to approve.");
        }


        foreach (var payroll in payrolls)
        {
            payroll.Status =
                PayrollStatuses.Approved;
        }


        period.Status =
            PayrollPeriodStatuses.Approved;


        await _context.SaveChangesAsync();

        return payrolls.Count;
    }


    // =========================================================
    // FINALIZE ALL
    // =========================================================

    public async Task<int>
        FinalizeAllAsync(
            int payrollPeriodId)
    {
        var period =
            await _context.PayrollPeriods
                .FirstOrDefaultAsync(
                    x => x.Id ==
                        payrollPeriodId);

        if (period == null)
        {
            throw new InvalidOperationException(
                "Payroll period was not found.");
        }

        if (period.Status !=
            PayrollPeriodStatuses.Approved)
        {
            throw new InvalidOperationException(
                "Payroll period must be Approved before finalization.");
        }


        // -----------------------------------------------------
        // EVERY PAYROLL MUST BE APPROVED
        // -----------------------------------------------------

        var hasNonApprovedPayroll =
            await _context.Payrolls
                .AnyAsync(x =>
                    x.PayrollPeriodId ==
                        payrollPeriodId &&
                    x.Status !=
                        PayrollStatuses.Approved);

        if (hasNonApprovedPayroll)
        {
            throw new InvalidOperationException(
                "All payrolls must be approved before finalization.");
        }


        var payrolls =
            await _context.Payrolls
                .Where(x =>
                    x.PayrollPeriodId ==
                        payrollPeriodId &&
                    x.Status ==
                        PayrollStatuses.Approved)
                .ToListAsync();


        if (payrolls.Count == 0)
        {
            throw new InvalidOperationException(
                "There are no Approved payrolls to finalize.");
        }


        foreach (var payroll in payrolls)
        {
            payroll.Status =
                PayrollStatuses.Finalized;
        }


        period.Status =
            PayrollPeriodStatuses.Finalized;


        await _context.SaveChangesAsync();

        return payrolls.Count;
    }


    // =========================================================
    // VALIDATION
    // =========================================================

    private static void
        ValidateCreateModel(
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

        if (model.OtherDeductions < 0)
        {
            throw new InvalidOperationException(
                "Other deductions cannot be negative.");
        }
    }


    // =========================================================
    // PAYROLL PERIOD VALIDATION
    // =========================================================

    private async Task
        ValidatePayrollPeriodAsync(
            int? payrollPeriodId,
            DateTime payrollDate)
    {
        if (!payrollPeriodId.HasValue)
        {
            return;
        }


        var period =
            await _context.PayrollPeriods
                .FirstOrDefaultAsync(
                    x => x.Id ==
                        payrollPeriodId.Value);

        if (period == null)
        {
            throw new InvalidOperationException(
                "Payroll period was not found.");
        }


        if (period.Status !=
                PayrollPeriodStatuses.Open &&
            period.Status !=
                PayrollPeriodStatuses.Processing)
        {
            throw new InvalidOperationException(
                "Payroll can only be created in an Open or Processing period.");
        }


        if (payrollDate.Date <
                period.StartDate.Date ||
            payrollDate.Date >
                period.EndDate.Date)
        {
            throw new InvalidOperationException(
                "Payroll date must fall within the selected payroll period.");
        }
    }


    // =========================================================
    // PERIOD PROCESSING VALIDATION
    // =========================================================

    private static void
        ValidatePeriodForProcessing(
            PayrollPeriodEntity? period)
    {
        if (period == null)
        {
            return;
        }

        if (period.Status !=
            PayrollPeriodStatuses.Processing)
        {
            throw new InvalidOperationException(
                "Payroll period must be in Processing status.");
        }
    }


    // =========================================================
    // CREATE DEDUCTION ENTITY
    // =========================================================

    private static PayrollDeductionEntity
        CreateDeductionEntity(
            PayrollEntity payroll,
            PayrollDeductionModel deductions)
    {
        return new PayrollDeductionEntity
        {
            Payroll =
                payroll,

            Sss =
                deductions.Sss,

            PhilHealth =
                deductions.PhilHealth,

            PagIbig =
                deductions.PagIbig,

            WithholdingTax =
                deductions.WithholdingTax,

            OtherDeductions =
                deductions.OtherDeductions
        };
    }


    // =========================================================
    // UPDATE DEDUCTION ENTITY
    // =========================================================

    private static void
        UpdateDeductionEntity(
            PayrollDeductionEntity entity,
            PayrollDeductionModel deductions)
    {
        entity.Sss =
            deductions.Sss;

        entity.PhilHealth =
            deductions.PhilHealth;

        entity.PagIbig =
            deductions.PagIbig;

        entity.WithholdingTax =
            deductions.WithholdingTax;

        entity.OtherDeductions =
            deductions.OtherDeductions;
    }
}