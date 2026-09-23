using EmployeeManagement.Application.Departments.Interfaces;
using EmployeeManagement.Application.Departments.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Web.Controllers;

[Authorize(Roles = "Administrator,HR Manager")]
public class DepartmentsController : Controller
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(
        IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    public async Task<IActionResult> Index()
    {
        var departments =
            await _departmentService.GetAllAsync();

        return View(departments);
    }

    public async Task<IActionResult> Details(int id)
    {
        var department =
            await _departmentService.GetByIdAsync(id);

        if (department is null)
            return NotFound();

        return View(department);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new DepartmentCreateModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        DepartmentCreateModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _departmentService.CreateAsync(model);

            TempData["SuccessMessage"] =
                "Department created successfully.";

            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var department =
            await _departmentService.GetByIdAsync(id);

        if (department is null)
            return NotFound();

        var model = new DepartmentCreateModel
        {
            Code = department.Code,
            Name = department.Name,
            Description = department.Description,
            IsActive = department.IsActive
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
      int id,
      DepartmentCreateModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var updated =
                await _departmentService
                    .UpdateAsync(id, model);

            if (!updated)
                return NotFound();

            TempData["SuccessMessage"] =
                "Department updated successfully.";

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted =
                await _departmentService.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            TempData["SuccessMessage"] =
                "Department deleted successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}