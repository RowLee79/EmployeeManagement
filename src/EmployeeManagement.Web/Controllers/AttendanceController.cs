using EmployeeManagement.Application.Attendance.Interfaces;
using EmployeeManagement.Application.Attendance.Models;
using EmployeeManagement.Application.Employees.Interfaces;
using EmployeeManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Web.Controllers;

[Authorize(Roles = "Administrator,HR Manager,Manager")]
public class AttendanceController : Controller
{
    private readonly IAttendanceService _attendanceService;
    private readonly IEmployeeService _employeeService;

    public AttendanceController(
        IAttendanceService attendanceService,
        IEmployeeService employeeService)
    {
        _attendanceService = attendanceService;
        _employeeService = employeeService;
    }


    // =========================================================
    // INDEX
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Index(
        string? search,
        DateTime? dateFrom,
        DateTime? dateTo,
        string? status,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var result =
            await _attendanceService.GetPagedAsync(
                search,
                dateFrom,
                dateTo,
                status,
                pageNumber,
                pageSize);

        ViewBag.Search = search;

        ViewBag.Status = status;

        ViewBag.DateFromValue =
            dateFrom?.ToString("yyyy-MM-dd");

        ViewBag.DateToValue =
            dateTo?.ToString("yyyy-MM-dd");

        ViewBag.PageSize = pageSize;

        return View(result);
    }


    // =========================================================
    // DETAILS
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var attendance =
            await _attendanceService.GetByIdAsync(id);

        if (attendance is null)
        {
            return NotFound();
        }

        return View(attendance);
    }


    // =========================================================
    // CREATE - GET
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new AttendanceCreateModel
        {
            AttendanceDate = DateTime.Today,
            Status = AttendanceStatus.Present
        };

        await LoadEmployees();

        return View(model);
    }


    // =========================================================
    // CREATE - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        AttendanceCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadEmployees();

            return View(model);
        }

        try
        {
            await _attendanceService.CreateAsync(model);

            TempData["SuccessMessage"] =
                "Attendance record created successfully.";

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
    // EDIT - GET
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var attendance =
            await _attendanceService.GetByIdAsync(id);

        if (attendance is null)
        {
            return NotFound();
        }

        var model = new AttendanceCreateModel
        {
            EmployeeId =
                attendance.EmployeeId,

            AttendanceDate =
                attendance.AttendanceDate,

            TimeIn =
                attendance.TimeIn,

            TimeOut =
                attendance.TimeOut,

            Status =
                Enum.TryParse<AttendanceStatus>(
                    attendance.Status,
                    out var status)
                    ? status
                    : AttendanceStatus.Present
        };

        await LoadEmployees();

        ViewBag.AttendanceId = id;

        return View(model);
    }


    // =========================================================
    // EDIT - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        AttendanceCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadEmployees();

            ViewBag.AttendanceId = id;

            return View(model);
        }

        try
        {
            var updated =
                await _attendanceService.UpdateAsync(
                    id,
                    model);

            if (!updated)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] =
                "Attendance record updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            await LoadEmployees();

            ViewBag.AttendanceId = id;

            return View(model);
        }
    }


    // =========================================================
    // DELETE
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted =
            await _attendanceService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] =
            "Attendance record deleted successfully.";

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