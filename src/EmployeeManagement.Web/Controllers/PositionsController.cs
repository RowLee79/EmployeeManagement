using EmployeeManagement.Application.Departments.Interfaces;
using EmployeeManagement.Application.Positions.Interfaces;
using EmployeeManagement.Application.Positions.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Web.Controllers;

[Authorize(Roles = "Administrator,HR Manager")]
public class PositionsController : Controller
{
    private readonly IPositionService _positionService;
    private readonly IDepartmentService _departmentService;

    public PositionsController(
        IPositionService positionService,
        IDepartmentService departmentService)
    {
        _positionService = positionService;
        _departmentService = departmentService;
    }

    // =========================================================
    // INDEX
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var positions =
            await _positionService.GetAllAsync();

        return View(positions);
    }


    // =========================================================
    // DETAILS
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var position =
            await _positionService.GetByIdAsync(id);

        if (position is null)
        {
            return NotFound();
        }

        return View(position);
    }


    // =========================================================
    // CREATE - GET
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new PositionCreateModel
        {
            IsActive = true
        };

        await LoadDepartments(model);

        return View(model);
    }


    // =========================================================
    // CREATE - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        PositionCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadDepartments(model);

            return View(model);
        }

        try
        {
            await _positionService.CreateAsync(model);

            TempData["SuccessMessage"] =
                "Position created successfully.";

            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            await LoadDepartments(model);

            return View(model);
        }
    }


    // =========================================================
    // EDIT - GET
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var position =
            await _positionService.GetByIdAsync(id);

        if (position is null)
        {
            return NotFound();
        }

        var model = new PositionCreateModel
        {
            Code = position.Code,
            Name = position.Name,
            DepartmentId = position.DepartmentId,
            MinimumSalary = position.MinimumSalary,
            MaximumSalary = position.MaximumSalary,
            Description = position.Description,
            IsActive = position.IsActive
        };

        await LoadDepartments(model);

        return View(model);
    }


    // =========================================================
    // EDIT - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        PositionCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadDepartments(model);

            return View(model);
        }

        try
        {
            var updated =
                await _positionService.UpdateAsync(
                    id,
                    model);

            if (!updated)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] =
                "Position updated successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            await LoadDepartments(model);

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
        try
        {
            var deleted =
                await _positionService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] =
                "Position deleted successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] =
                ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }


    // =========================================================
    // POSITIONS BY DEPARTMENT
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> ByDepartment(
        int departmentId)
    {
        var positions =
            await _positionService
                .GetByDepartmentAsync(departmentId);

        return Json(
            positions.Select(x => new
            {
                id = x.Id,
                name = x.Name
            }));
    }


    // =========================================================
    // LOAD DEPARTMENTS
    // =========================================================

    private async Task LoadDepartments(
        PositionCreateModel model)
    {
        model.Departments =
            await _departmentService.GetLookupAsync();
    }
}