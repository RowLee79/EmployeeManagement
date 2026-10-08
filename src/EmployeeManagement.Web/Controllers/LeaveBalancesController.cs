using EmployeeManagement.Application.Employees.Interfaces;
using EmployeeManagement.Application.LeaveManagement.Interfaces;
using EmployeeManagement.Application.LeaveManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace EmployeeManagement.Web.Controllers;
[Authorize(Roles = "Administrator,HR Manager")]
public sealed class LeaveBalancesController(ILeaveAllocationService allocations, ILeaveService leaves,
    IEmployeeService employees) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(int? employeeId, int? year)
    {
        var model = new LeaveAllocationModel { EmployeeId = employeeId ?? 0, Year = year ?? DateTime.Today.Year };
        if (model.Year is < 2000 or > 2100) model.Year = DateTime.Today.Year;
        await LoadAsync(model);
        return View(model);
    }
    [HttpPost]
    public async Task<IActionResult> Index(LeaveAllocationModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await allocations.SetAllocationAsync(model);
                TempData["SuccessMessage"] = "Leave allocation saved.";
                return RedirectToAction(nameof(Index), new { employeeId = model.EmployeeId, year = model.Year });
            }
            catch (InvalidOperationException ex) { ModelState.AddModelError("", ex.Message); }
        }
        await LoadAsync(model);
        return View(model);
    }
    private async Task LoadAsync(LeaveAllocationModel model)
    {
        ViewBag.Employees = await employees.GetLookupAsync();
        ViewBag.Balances = await leaves.GetEmployeeBalancesAsync(model.EmployeeId, model.Year);
    }
}
