using EmployeeManagement.Application.Payroll.Interfaces;
using EmployeeManagement.Application.Payroll.Models;
using EmployeeManagement.Application.Payroll;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Web.Controllers;

[Authorize(Roles = "Administrator,HR Manager")]
public class PayrollPeriodController : Controller
{
    private readonly IPayrollPeriodService
        _payrollPeriodService;

    private readonly IPayrollService
        _payrollService;

    public PayrollPeriodController(
        IPayrollPeriodService payrollPeriodService,
        IPayrollService payrollService)
    {
        _payrollPeriodService =
            payrollPeriodService;

        _payrollService =
            payrollService;
    }


    // =========================================================
    // INDEX
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var periods =
            await _payrollPeriodService
                .GetAllAsync();

        return View(periods);
    }


    // =========================================================
    // DETAILS
    // =========================================================

    [HttpGet]
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var period =
            await _payrollPeriodService.GetByIdAsync(id);

        if (period == null)
        {
            return NotFound();
        }

        return View(period);
    }


    // =========================================================
    // CREATE - GET
    // =========================================================

    [HttpGet]
    public IActionResult Create()
    {
        var model =
            new PayrollPeriodCreateModel
            {
                StartDate =
                    DateTime.Today,

                EndDate =
                    DateTime.Today,

                PayDate =
                    DateTime.Today
            };

        return View(model);
    }


    // =========================================================
    // CREATE - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        PayrollPeriodCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var id =
                await _payrollPeriodService
                    .CreateAsync(model);

            TempData["Success"] =
                "Payroll period created successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            return View(model);
        }
    }


    // =========================================================
    // EDIT - GET
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Edit(
        int id)
    {
        var period =
            await _payrollPeriodService
                .GetByIdAsync(id);

        if (period == null)
        {
            return NotFound();
        }


        if (period.Status !=
            PayrollPeriodStatuses.Open)
        {
            TempData["Error"] =
                "Only Open payroll periods can be edited.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }


        var model =
            new PayrollPeriodCreateModel
            {
                PeriodCode =
                    period.PeriodCode,

                StartDate =
                    period.StartDate,

                EndDate =
                    period.EndDate,

                PayDate =
                    period.PayDate,

                Description =
                    period.Description
            };

        return View(model);
    }


    // =========================================================
    // EDIT - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        PayrollPeriodCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }


        try
        {
            var updated =
                await _payrollPeriodService
                    .UpdateAsync(
                        id,
                        model);

            if (!updated)
            {
                return NotFound();
            }


            TempData["Success"] =
                "Payroll period updated successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            return View(model);
        }
    }


    // =========================================================
    // DELETE
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        int id)
    {
        try
        {
            var deleted =
                await _payrollPeriodService
                    .DeleteAsync(id);

            if (!deleted)
            {
                TempData["Error"] =
                    "Payroll period could not be deleted.";

                return RedirectToAction(
                    nameof(Index));
            }


            TempData["Success"] =
                "Payroll period deleted successfully.";

            return RedirectToAction(
                nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] =
                ex.Message;

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
    }


    // =========================================================
    // START PROCESSING
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartProcessing(
        int id)
    {
        try
        {
            var success =
                await _payrollPeriodService
                    .StartProcessingAsync(id);

            if (!success)
            {
                TempData["Error"] =
                    "Payroll period could not be moved to Processing.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            TempData["Success"] =
                "Payroll period is now Processing.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] =
                ex.Message;

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
    }


    // =========================================================
    // GENERATE PAYROLL
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GeneratePayroll(
        int id)
    {
        try
        {
            var result =
                await _payrollService
                    .GeneratePayrollAsync(id);


            TempData["Success"] =
                $"Payroll generation completed. " +
                $"Employees processed: " +
                $"{result.EmployeesProcessed}. " +
                $"Created: " +
                $"{result.PayrollsCreated}. " +
                $"Skipped: " +
                $"{result.PayrollsSkipped}.";


            return RedirectToAction(
                nameof(Details),
                new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] =
                ex.Message;

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
    }


    // =========================================================
    // CALCULATE ALL
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CalculateAll(
        int id)
    {
        try
        {
            var count =
                await _payrollService
                    .CalculateAllAsync(id);


            TempData["Success"] =
                $"{count} payroll(s) calculated successfully.";


            return RedirectToAction(
                nameof(Details),
                new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] =
                ex.Message;

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
    }


    // =========================================================
    // APPROVE ALL
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveAll(
        int id)
    {
        try
        {
            var count =
                await _payrollService
                    .ApproveAllAsync(id);


            TempData["Success"] =
                $"{count} payroll(s) approved successfully.";


            return RedirectToAction(
                nameof(Details),
                new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] =
                ex.Message;

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
    }


    // =========================================================
    // FINALIZE ALL
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FinalizeAll(
        int id)
    {
        try
        {
            var count =
                await _payrollService
                    .FinalizeAllAsync(id);


            TempData["Success"] =
                $"{count} payroll(s) finalized successfully.";


            return RedirectToAction(
                nameof(Details),
                new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] =
                ex.Message;

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
    }


    // =========================================================
    // APPROVE PERIOD
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(
        int id)
    {
        try
        {
            var success =
                await _payrollPeriodService
                    .ApproveAsync(id);

            if (!success)
            {
                TempData["Error"] =
                    "Payroll period could not be approved.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            TempData["Success"] =
                "Payroll period approved successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] =
                ex.Message;

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
    }


    // =========================================================
    // FINALIZE PERIOD
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Finalize(
        int id)
    {
        try
        {
            var success =
                await _payrollPeriodService
                    .FinalizeAsync(id);

            if (!success)
            {
                TempData["Error"] =
                    "Payroll period could not be finalized.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            TempData["Success"] =
                "Payroll period finalized successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] =
                ex.Message;

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
    }


    // =========================================================
    // CLOSE PERIOD
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(
        int id)
    {
        try
        {
            var success =
                await _payrollPeriodService
                    .CloseAsync(id);

            if (!success)
            {
                TempData["Error"] =
                    "Payroll period could not be closed.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            TempData["Success"] =
                "Payroll period closed successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] =
                ex.Message;

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
    }


    // =========================================================
    // LOCK PERIOD
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lock(
        int id)
    {
        try
        {
            var success =
                await _payrollPeriodService
                    .LockAsync(id);

            if (!success)
            {
                TempData["Error"] =
                    "Payroll period could not be locked.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            TempData["Success"] =
                "Payroll period locked successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] =
                ex.Message;

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
    }
}