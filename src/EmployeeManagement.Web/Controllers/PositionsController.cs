using EmployeeManagement.Application.Departments.Interfaces;
using EmployeeManagement.Application.Positions.Interfaces;
using EmployeeManagement.Application.Positions.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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

    public async Task<IActionResult> Index()
    {
        var positions =
            await _positionService.GetAllAsync();

        return View(positions);
    }

    public async Task<IActionResult> Details(int id)
    {
        var position =
            await _positionService.GetByIdAsync(id);

        if (position is null)
            return NotFound();

        return View(position);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadDepartments();

        return View(new PositionCreateModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        PositionCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadDepartments();
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

            await LoadDepartments();

            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var position =
            await _positionService.GetByIdAsync(id);

        if (position is null)
            return NotFound();

        ViewBag.Departments =
            new SelectList(
                await _departmentService.GetLookupAsync(),
                "Id",
                "Name",
                position.DepartmentId);

        var model = new PositionCreateModel
        {
            Code = position.Code,
            Name = position.Name,
            Description = position.Description,
            MinimumSalary = position.MinimumSalary,
            MaximumSalary = position.MaximumSalary,
            IsActive = position.IsActive,
            DepartmentId = position.DepartmentId
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
       int id,
       PositionCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadDepartments();
            return View(model);
        }

        try
        {
            var updated =
                await _positionService
                    .UpdateAsync(id, model);

            if (!updated)
                return NotFound();

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

            await LoadDepartments();

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
                await _positionService.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            TempData["SuccessMessage"] =
                "Position deleted successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadDepartments()
    {
        ViewBag.Departments =
            new SelectList(
                await _departmentService.GetLookupAsync(),
                "Id",
                "Name");
    }
    [HttpGet]
    public async Task<IActionResult> ByDepartment(int departmentId)
    {
        var positions =
            await _positionService.GetByDepartmentAsync(departmentId);

        return Json(positions.Select(x => new
        {
            id = x.Id,
            name = x.Name
        }));
    }
}