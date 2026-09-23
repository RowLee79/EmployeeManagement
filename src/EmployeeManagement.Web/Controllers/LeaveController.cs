using EmployeeManagement.Application.Employees.Interfaces;
using EmployeeManagement.Application.LeaveManagement.Interfaces;
using EmployeeManagement.Application.LeaveManagement.Models;
using EmployeeManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Web.Controllers;

[Authorize]
public class LeaveController : Controller
{
    private readonly ILeaveService _leaveService;
    private readonly IEmployeeService _employeeService;

    public LeaveController(
        ILeaveService leaveService,
        IEmployeeService employeeService)
    {
        _leaveService = leaveService;
        _employeeService = employeeService;
    }

    // =========================================================
    // INDEX
    // =========================================================
    [HttpGet]
    public async Task<IActionResult> Index(
        string? search,
        LeaveType? leaveType,
        LeaveStatus? status,
        DateTime? dateFrom,
        DateTime? dateTo,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var result = await _leaveService.GetPagedAsync(
            search,
            leaveType,
            status,
            dateFrom,
            dateTo,
            pageNumber,
            pageSize);

        ViewBag.Search = search;

        ViewBag.LeaveType = leaveType;

        ViewBag.Status = status;

        ViewBag.DateFrom =
            dateFrom?.ToString("yyyy-MM-dd");

        ViewBag.DateTo =
            dateTo?.ToString("yyyy-MM-dd");

        ViewBag.PageNumber = pageNumber;

        ViewBag.PageSize = pageSize;

        return View(result);
    }

    // =========================================================
    // DETAILS
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var leave = await _leaveService.GetByIdAsync(id);

        if (leave == null)
            return NotFound();

        return View(leave);
    }

    // =========================================================
    // CREATE
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadEmployees();

        var model = new LeaveRequestCreateModel
        {
            StartDate = DateTime.Today,
            EndDate = DateTime.Today,
            LeaveType = LeaveType.Vacation
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        LeaveRequestCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadEmployees();
            return View(model);
        }

        try
        {
            await _leaveService.CreateAsync(model);

            TempData["SuccessMessage"] =
                "Leave request submitted successfully.";

            return RedirectToAction(nameof(Index));
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

    // =========================================================
    // EDIT
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var leave = await _leaveService.GetByIdAsync(id);

        if (leave == null)
            return NotFound();

        await LoadEmployees();

        var model = new LeaveRequestCreateModel
        {
            EmployeeId = leave.EmployeeId,
            LeaveType = leave.LeaveType,
            StartDate = leave.StartDate,
            EndDate = leave.EndDate,
            Reason = leave.Reason
        };

        ViewBag.LeaveId = id;

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        LeaveRequestCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadEmployees();
            ViewBag.LeaveId = id;

            return View(model);
        }

        var updated =
            await _leaveService.UpdateAsync(id, model);

        if (!updated)
            return NotFound();

        TempData["SuccessMessage"] =
            "Leave request updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    // =========================================================
    // APPROVE
    // =========================================================

    [HttpPost]
    [Authorize(Roles = "Administrator,HR Manager,Manager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var approvedBy =
            User.Identity?.Name ?? "System";

        var success =
            await _leaveService.ApproveAsync(
                id,
                approvedBy);

        if (!success)
            return NotFound();

        TempData["SuccessMessage"] =
            "Leave request approved successfully.";

        return RedirectToAction(nameof(Index));
    }

    // =========================================================
    // REJECT
    // =========================================================

    [HttpPost]
    [Authorize(Roles = "Administrator,HR Manager,Manager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(
     int id,
     string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["ErrorMessage"] =
                "A rejection reason is required.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        var rejectedBy =
            User.Identity?.Name ?? "System";

        var success =
            await _leaveService.RejectAsync(
                id,
                rejectedBy,
                reason);

        if (!success)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] =
            "Leave request rejected successfully.";

        return RedirectToAction(nameof(Index));
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var cancelled =
            await _leaveService.CancelAsync(id);

        if (!cancelled)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] =
            "Leave request cancelled successfully.";

        return RedirectToAction(nameof(Index));
    }

    // =========================================================
    // EMPLOYEE LOOKUP
    // =========================================================

    private async Task LoadEmployees()
    {
        ViewBag.Employees =
            await _employeeService.GetLookupAsync();
    }
}