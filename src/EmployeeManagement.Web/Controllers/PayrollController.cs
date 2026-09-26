using EmployeeManagement.Application.Payroll.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Web.Controllers;

[Authorize(Roles = "Administrator,HR Manager")]
public class PayrollController : Controller
{
    private readonly IPayrollService _payrollService;

    public PayrollController(
        IPayrollService payrollService)
    {
        _payrollService = payrollService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var payrolls =
            await _payrollService.GetAllAsync();

        var dashboard =
            await _payrollService.GetDashboardAsync();

        ViewBag.Dashboard = dashboard;

        return View(payrolls);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var payroll =
            await _payrollService.GetByIdAsync(id);

        if (payroll is null)
        {
            return NotFound();
        }

        return View(payroll);
    }
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new PayrollCreateModel
        {
            PayrollDate = DateTime.Today
        };

        await LoadEmployees();

        return View(model);
    }

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
            var id =
                await _payrollService
                    .CreateAsync(model);

            TempData["SuccessMessage"] =
                "Payroll created successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            await LoadEmployees();

            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var payroll =
            await _payrollService.GetByIdAsync(id);

        if (payroll is null)
        {
            return NotFound();
        }

        if (payroll.Status != "Draft")
        {
            TempData["ErrorMessage"] =
                "Only Draft payroll can be edited.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        var model = new PayrollCreateModel
        {
            EmployeeId = payroll.EmployeeId,

            PayrollDate = payroll.PayrollDate,

            BasicSalary = payroll.BasicSalary,

            Overtime = payroll.Overtime,

            Allowances = payroll.Allowances,

            Deductions = payroll.Deductions,

            Status = payroll.Status
        };

        await LoadEmployees();

        return View(model);
    }

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

            TempData["SuccessMessage"] =
                "Payroll updated successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            await LoadEmployees();

            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
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

            TempData["SuccessMessage"] =
                "Payroll deleted successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] =
                ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadEmployees()
    {
        ViewBag.Employees =
            await _payrollService
                .GetEmployeeLookupAsync();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Calculate(int id)
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

            TempData["SuccessMessage"] =
                "Payroll calculated successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] =
                ex.Message;
        }

        return RedirectToAction(
            nameof(Details),
            new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
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

            TempData["SuccessMessage"] =
                "Payroll approved successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] =
                ex.Message;
        }

        return RedirectToAction(
            nameof(Details),
            new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Finalize(int id)
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

            TempData["SuccessMessage"] =
                "Payroll finalized successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] =
                ex.Message;
        }

        return RedirectToAction(
            nameof(Details),
            new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
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

            TempData["SuccessMessage"] =
                "Payroll cancelled successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] =
                ex.Message;
        }

        return RedirectToAction(
            nameof(Details),
            new { id });
    }
}