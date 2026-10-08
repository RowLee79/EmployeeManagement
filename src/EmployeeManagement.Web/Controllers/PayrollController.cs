using EmployeeManagement.Application.Payroll;
using EmployeeManagement.Application.Payroll.Interfaces;
using EmployeeManagement.Application.Payroll.Models;
using EmployeeManagement.Web.Services.Payroll;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Web.Controllers;

[Authorize(Roles = "Administrator,HR Manager")]
public class PayrollController : Controller
{
    private readonly IPayrollService _payrollService;
    private readonly IPayslipPdfService _payslipPdfService;
    private readonly IPayrollPeriodService _payrollPeriodService;
    public PayrollController(
        IPayrollService payrollService,
        IPayslipPdfService payslipPdfService,
        IPayrollPeriodService payrollPeriodService)
    {
        _payrollService = payrollService;
        _payslipPdfService = payslipPdfService;
        _payrollPeriodService = payrollPeriodService;
    }


    // ============================================================
    // INDEX
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Index(
        PayrollSearchModel searchModel)
    {
        var payrolls =
            await _payrollService
                .GetPagedAsync(searchModel);

        var dashboard =
            await _payrollService
                .GetDashboardAsync();

        var model =
            new PayrollIndexViewModel
            {
                Payrolls = payrolls,

                Search = searchModel.Search,

                Status = searchModel.Status,

                PayrollDateFrom =
                    searchModel.PayrollDateFrom,

                PayrollDateTo =
                    searchModel.PayrollDateTo,

                PageNumber =
                    searchModel.PageNumber,

                PageSize =
                    searchModel.PageSize,

                SortBy =
                    searchModel.SortBy,

                SortDescending =
                    searchModel.SortDescending,

                Dashboard = dashboard
            };

        return View(model);
    }


    // ============================================================
    // DETAILS
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var payroll =
            await _payrollService
                .GetByIdAsync(id);

        if (payroll == null)
        {
            return NotFound();
        }

        return View(payroll);
    }


    // ============================================================
    // CREATE - GET
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadEmployeesAsync();

        await LoadPayrollPeriodsAsync();

        return View(
            new PayrollCreateModel
            {
                PayrollDate = DateTime.Today,
                Status = PayrollStatuses.Draft
            });
    }


    // ============================================================
    // CREATE - POST
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        PayrollCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadEmployees();

            return View(model);
        }

        try
        {
            var payrollId =
                await _payrollService
                    .CreateAsync(model);

            TempData["Success"] =
                "Payroll created successfully.";

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id = payrollId
                });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            await LoadEmployees();

            return View(model);
        }
    }


    // ============================================================
    // EDIT - GET
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var payroll =
            await _payrollService
                .GetByIdAsync(id);

        if (payroll == null)
        {
            return NotFound();
        }

        if (payroll.Status !=
            PayrollStatuses.Draft)
        {
            TempData["Error"] =
                "Only Draft payroll can be edited.";

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id
                });
        }

        await LoadEmployees();

        var model =
            new PayrollCreateModel
            {
                EmployeeId =
                    payroll.EmployeeId,

                PayrollDate =
                    payroll.PayrollDate,

                BasicSalary =
                    payroll.BasicSalary,

                Overtime =
                    payroll.Overtime,

                Allowances =
                    payroll.Allowances,

                OtherDeductions =
                    payroll.Deductions,

                Status =
                    payroll.Status
            };

        return View(model);
    }


    // ============================================================
    // EDIT - POST
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        PayrollCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadEmployees();

            return View(model);
        }

        try
        {
            var updated =
                await _payrollService
                    .UpdateAsync(id, model);

            if (!updated)
            {
                return NotFound();
            }

            TempData["Success"] =
                "Payroll updated successfully.";

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id
                });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            await LoadEmployees();

            return View(model);
        }
    }


    // ============================================================
    // DELETE
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        int id)
    {
        try
        {
            var deleted =
                await _payrollService
                    .DeleteAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            TempData["Success"] =
                "Payroll deleted successfully.";

            return RedirectToAction(
                nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] =
                ex.Message;

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id
                });
        }
    }


    // ============================================================
    // CALCULATE
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Calculate(
        int id)
    {
        try
        {
            var result =
                await _payrollService
                    .CalculateAsync(id);

            if (!result)
            {
                return NotFound();
            }

            TempData["Success"] =
                "Payroll calculated successfully.";

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id
                });
        }
        catch (Exception ex)
        {
            TempData["Error"] =
                ex.Message;

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id
                });
        }
    }


    // ============================================================
    // APPROVE
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(
        int id)
    {
        try
        {
            var result =
                await _payrollService
                    .ApproveAsync(id);

            if (!result)
            {
                return NotFound();
            }

            TempData["Success"] =
                "Payroll approved successfully.";

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id
                });
        }
        catch (Exception ex)
        {
            TempData["Error"] =
                ex.Message;

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id
                });
        }
    }


    // ============================================================
    // FINALIZE
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Finalize(
        int id)
    {
        try
        {
            var result =
                await _payrollService
                    .FinalizeAsync(id);

            if (!result)
            {
                return NotFound();
            }

            TempData["Success"] =
                "Payroll finalized successfully.";

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id
                });
        }
        catch (Exception ex)
        {
            TempData["Error"] =
                ex.Message;

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id
                });
        }
    }


    // ============================================================
    // CANCEL
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(
        int id)
    {
        try
        {
            var result =
                await _payrollService
                    .CancelAsync(id);

            if (!result)
            {
                return NotFound();
            }

            TempData["Success"] =
                "Payroll cancelled successfully.";

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id
                });
        }
        catch (Exception ex)
        {
            TempData["Error"] =
                ex.Message;

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id
                });
        }
    }


    // ============================================================
    // EMPLOYEE PAYROLL HISTORY
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> EmployeeHistory(
        int employeeId)
    {
        var history =
            await _payrollService
                .GetEmployeeHistoryAsync(
                    employeeId);

        if (history == null ||
            history.Count == 0)
        {
            return NotFound(
                "No payroll history was found.");
        }

        return View(history);
    }


    // ============================================================
    // PAYSLIP
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Payslip(
        int id)
    {
        var payslip =
            await _payrollService
                .GetPayslipAsync(id);

        if (payslip == null)
        {
            return NotFound();
        }

        if (payslip.Status !=
                PayrollStatuses.Approved &&
            payslip.Status !=
                PayrollStatuses.Finalized)
        {
            TempData["Error"] =
                "Payslip is available only for " +
                "Approved or Finalized payroll.";

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id
                });
        }

        return View(payslip);
    }


    // ============================================================
    // PAYSLIP PDF
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> PayslipPdf(
        int id)
    {
        var payslip =
            await _payrollService
                .GetPayslipAsync(id);

        if (payslip == null)
        {
            return NotFound();
        }

        if (payslip.Status !=
                PayrollStatuses.Approved &&
            payslip.Status !=
                PayrollStatuses.Finalized)
        {
            TempData["Error"] =
                "Payslip PDF is available only for " +
                "Approved or Finalized payroll.";

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id
                });
        }

        var pdf =
            _payslipPdfService
                .GeneratePayslip(payslip);

        var employeeNumber =
            string.IsNullOrWhiteSpace(
                payslip.EmployeeNumber)
                ? payslip.EmployeeId.ToString()
                : payslip.EmployeeNumber;

        var fileName =
            $"Payslip-{employeeNumber}-" +
            $"{payslip.PayrollDate:yyyy-MM-dd}.pdf";

        return File(
            pdf,
            "application/pdf",
            fileName);
    }


    // ============================================================
    // LOAD EMPLOYEES
    // ============================================================

    private async Task LoadEmployees()
    {
        var employees =
            await _payrollService
                .GetEmployeeLookupAsync();

        ViewBag.Employees =
            employees;
    }

    private async Task LoadPayrollPeriodsAsync()
    {
        ViewBag.PayrollPeriods =
            await _payrollPeriodService
                .GetOpenLookupAsync();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(
    int payrollPeriodId)
    {
        try
        {
            var result =
                await _payrollService
                    .GeneratePayrollAsync(
                        payrollPeriodId);

            TempData["Success"] =
                $"Payroll generation completed. " +
                $"{result.PayrollsCreated} payroll(s) created, " +
                $"{result.PayrollsSkipped} skipped.";

            return RedirectToAction(
                "Details",
                "PayrollPeriod",
                new
                {
                    id = payrollPeriodId
                });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;

            return RedirectToAction(
                "Details",
                "PayrollPeriod",
                new
                {
                    id = payrollPeriodId
                });
        }
    }

    private async Task LoadEmployeesAsync()
    {
        ViewBag.Employees =
            await _payrollService.GetEmployeeLookupAsync();
    }
}