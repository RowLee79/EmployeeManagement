using EmployeeManagement.Application.Departments.Interfaces;
using EmployeeManagement.Application.Employees.Interfaces;
using EmployeeManagement.Application.Employees.Models;
using EmployeeManagement.Application.Positions.Interfaces;
using EmployeeManagement.Application.Positions.Models;
using EmployeeManagement.Application.Reporting;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Web.Controllers;

[Authorize]
public class EmployeesController : Controller
{
    private readonly IEmployeeService _employeeService;
    private readonly IDepartmentService _departmentService;
    private readonly IPositionService _positionService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IReportingService _reportingService;

    public EmployeesController(
        IEmployeeService employeeService,
        IDepartmentService departmentService,
        IPositionService positionService,
        IFileStorageService fileStorageService,
        IReportingService reportingService)
    {
        _employeeService = employeeService;
        _departmentService = departmentService;
        _positionService = positionService;
        _fileStorageService = fileStorageService;
        _reportingService = reportingService;
    }

    // =========================================================
    // INDEX
    // =========================================================

    [HttpGet]
    [Authorize(Roles = "Administrator,HR Manager,Manager")]
    public async Task<IActionResult> Index(EmployeeSearchModel searchModel)
    {
        var result = await _employeeService.GetPagedAsync(searchModel);

        var departments =
            await _departmentService.GetLookupAsync();

        var positions =
            await _positionService.GetLookupAsync();

        ViewBag.Search = searchModel.Search;
        ViewBag.DepartmentId = searchModel.DepartmentId;
        ViewBag.PositionId = searchModel.PositionId;
        ViewBag.Status = searchModel.Status;
        ViewBag.EmploymentType = searchModel.EmploymentType;
        ViewBag.HireDateFrom = searchModel.HireDateFrom;
        ViewBag.HireDateTo = searchModel.HireDateTo;
        ViewBag.SortBy = searchModel.SortBy;
        ViewBag.SortDescending = searchModel.SortDescending;

        ViewBag.Departments = departments;
        ViewBag.Positions = positions;

        return View(result);
    }


    // =========================================================
    // DETAILS
    // =========================================================

    [HttpGet]
    [Authorize(Roles = "Administrator,HR Manager,Manager")]
    public async Task<IActionResult> Details(int id)
    {
        var employee =
            await _employeeService.GetByIdAsync(id);

        if (employee == null)
        {
            return NotFound();
        }

        return View(employee);
    }

    // =========================================================
    // CREATE - GET
    // =========================================================

    [HttpGet]
    [Authorize(Roles = "Administrator,HR Manager")]
    public async Task<IActionResult> Create()
    {
        await LoadDepartments();

        ViewBag.Positions =
            Array.Empty<PositionListModel>();

        var model = new EmployeeCreateModel
        {
            HireDate = DateTime.Today,

            Status = EmployeeStatus.Active,

            EmploymentType =
                EmploymentType.Regular,

            Gender = Gender.Male
        };

        return View(model);
    }

    // =========================================================
    // CREATE - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator,HR Manager")]
    public async Task<IActionResult> Create(EmployeeCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadDepartments();

            if (model.DepartmentId > 0)
                await LoadPositions(model.DepartmentId);
            else
                ViewBag.Positions = Array.Empty<PositionListModel>();

            return View(model);
        }

        try
        {
            if (model.ProfileImageFile != null)
            {
                model.ProfileImage =
                    await _fileStorageService.SaveEmployeeProfileImageAsync(
                        model.ProfileImageFile);
            }

            var employeeId = await _employeeService.CreateAsync(model);

            TempData["SuccessMessage"] =
                "Employee created successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id = employeeId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            await LoadDepartments();

            if (model.DepartmentId > 0)
                await LoadPositions(model.DepartmentId);
            else
                ViewBag.Positions = Array.Empty<PositionListModel>();

            return View(model);
        }
    }

    // =========================================================
    // EDIT - GET
    // =========================================================

    [HttpGet]
    [Authorize(Roles = "Administrator,HR Manager")]
    public async Task<IActionResult> Edit(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);

        if (employee == null)
            return NotFound();

        var model = new EmployeeEditModel
        {
            EmployeeNumber = employee.EmployeeNumber,
            FirstName = employee.FirstName,
            MiddleName = employee.MiddleName,
            LastName = employee.LastName,
            Suffix = employee.Suffix,
            BirthDate = employee.BirthDate,
            Gender = employee.Gender,
            CivilStatus = employee.CivilStatus.ToString(),
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            Address = employee.Address,
            ProfileImage = employee.ProfileImage,
            HireDate = employee.HireDate,
            RegularizationDate = employee.RegularizationDate,
            EmploymentType = employee.EmploymentType,
            Status = employee.Status,
            BasicSalary = employee.BasicSalary,
            DepartmentId = employee.DepartmentId,
            PositionId = employee.PositionId
        };

        await LoadDepartments();
        await LoadPositions(employee.DepartmentId);

        ViewBag.EmployeeId = id;

        return View(model);
    }

    // =========================================================
    // EDIT - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator,HR Manager")]
    public async Task<IActionResult> Edit(
     int id,
     EmployeeEditModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadDepartments();

            if (model.DepartmentId > 0)
                await LoadPositions(model.DepartmentId);
            else
                ViewBag.Positions = Array.Empty<PositionListModel>();

            ViewBag.EmployeeId = id;

            return View(model);
        }

        try
        {
            // Get existing employee first
            var existingEmployee =
                await _employeeService.GetByIdAsync(id);

            if (existingEmployee == null)
                return NotFound();

            // Upload new image if supplied
            if (model.ProfileImageFile != null)
            {
                var oldImage = existingEmployee.ProfileImage;

                model.ProfileImage =
                    await _fileStorageService.SaveEmployeeProfileImageAsync(
                        model.ProfileImageFile);

                // Delete old image
                if (!string.IsNullOrWhiteSpace(oldImage))
                {
                    await _fileStorageService.DeleteAsync(oldImage);
                }
            }
            else
            {
                // Keep existing image
                model.ProfileImage =
                    existingEmployee.ProfileImage;
            }

            var success =
                await _employeeService.UpdateAsync(id, model);

            if (!success)
                return NotFound();

            TempData["SuccessMessage"] =
                "Employee updated successfully.";

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

            if (model.DepartmentId > 0)
                await LoadPositions(model.DepartmentId);
            else
                ViewBag.Positions = Array.Empty<PositionListModel>();

            ViewBag.EmployeeId = id;

            return View(model);
        }
    }

    // =========================================================
    // DELETE
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator,HR Manager")]
    public async Task<IActionResult> Delete(int id)
    {
        var success =
            await _employeeService.DeleteAsync(id);

        if (!success)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] =
            "Employee deleted successfully.";

        return RedirectToAction(nameof(Index));
    }

    // =========================================================
    // EMPLOYEE LOOKUP
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Lookup()
    {
        var employees =
            await _employeeService.GetLookupAsync();

        return Json(employees);
    }

    // =========================================================
    // POSITIONS BY DEPARTMENT
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> ByDepartment(
        int departmentId)
    {
        if (departmentId <= 0)
        {
            return Json(
                Array.Empty<PositionListModel>());
        }

        var positions =
            await _positionService
                .GetByDepartmentAsync(departmentId);

        return Json(positions);
    }

    // =========================================================
    // LOAD DEPARTMENTS
    // =========================================================

    private async Task LoadDepartments()
    {
        ViewBag.Departments =
            await _departmentService.GetLookupAsync();
    }

    // =========================================================
    // LOAD POSITIONS
    // =========================================================

    private async Task LoadPositions(
        int departmentId)
    {
        if (departmentId <= 0)
        {
            ViewBag.Positions =
                Array.Empty<PositionListModel>();

            return;
        }

        ViewBag.Positions =
            await _positionService
                .GetByDepartmentAsync(departmentId);
    }

    [HttpGet]
    [Authorize(Roles = "Administrator,HR Manager,Manager")]
    public async Task<IActionResult> ExportCsv(
    string? search,
    int? departmentId,
    int? positionId,
    EmployeeStatus? status,
    EmploymentType? employmentType,
    DateTime? hireDateFrom,
    DateTime? hireDateTo,
    string sortBy = "HireDate",
    bool sortDescending = true)
    {
        var searchModel = new EmployeeSearchModel
        {
            Search = search,
            DepartmentId = departmentId,
            PositionId = positionId,
            Status = status,
            EmploymentType = employmentType,
            HireDateFrom = hireDateFrom,
            HireDateTo = hireDateTo,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var fileBytes =
            await _employeeService.ExportCsvAsync(searchModel);

        var fileName =
            $"employees_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

        return File(
            fileBytes,
            "text/csv",
            fileName);
    }

    [HttpGet]
    public async Task<IActionResult> EmployeeMasterReport(
     EmployeeSearchModel searchModel)
    {
        var reportRequest = new EmployeeReportRequest
        {
            Search = searchModel.Search,

            DepartmentId = searchModel.DepartmentId,

            PositionId = searchModel.PositionId,

            Status = searchModel.Status.HasValue
                ? (int)searchModel.Status.Value
                : null,

            EmploymentType =
                searchModel.EmploymentType.HasValue
                    ? (int)searchModel.EmploymentType.Value
                    : null,

            HireDateFrom = searchModel.HireDateFrom,

            HireDateTo = searchModel.HireDateTo
        };

        var pdf =
            await _reportingService
                .GetEmployeeMasterListPdfAsync(
                    reportRequest);

        return File(
            pdf,
            "application/pdf",
            "EmployeeMasterList.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> Search(
    EmployeeSearchModel searchModel)
    {
        var result =
            await _employeeService.GetPagedAsync(searchModel);

        ViewBag.Search = searchModel.Search;
        ViewBag.DepartmentId = searchModel.DepartmentId;
        ViewBag.PositionId = searchModel.PositionId;
        ViewBag.Status = searchModel.Status;
        ViewBag.EmploymentType = searchModel.EmploymentType;
        ViewBag.HireDateFrom = searchModel.HireDateFrom;
        ViewBag.HireDateTo = searchModel.HireDateTo;

        ViewBag.SortBy = searchModel.SortBy;
        ViewBag.SortDescending = searchModel.SortDescending;

        return PartialView(
            "_EmployeeResults",
            result);
    }
}