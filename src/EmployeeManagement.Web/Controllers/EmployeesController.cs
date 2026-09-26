using Azure.Core;
using EmployeeManagement.Application.Departments.Interfaces;
using EmployeeManagement.Application.Employees.Interfaces;
using EmployeeManagement.Application.Employees.Models;
using EmployeeManagement.Application.Positions.Interfaces;
using EmployeeManagement.Application.Positions.Models;
using EmployeeManagement.Application.Reporting;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Web.Models.Employees;
using EmployeeManagement.Web.Services;
using EmployeeManagement.Web.Services.Reporting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Web.Controllers;

[Authorize(Roles = "Administrator,HR Manager,Manager")]
public class EmployeesController : Controller
{
    private readonly IEmployeeService _employeeService;
    private readonly IDepartmentService _departmentService;
    private readonly IPositionService _positionService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IReportingService _reportingService;
    private readonly IWebHostEnvironment _environment;

    public EmployeesController(
        IEmployeeService employeeService,
        IDepartmentService departmentService,
        IPositionService positionService,
        IFileStorageService fileStorageService,
        IReportingService reportingService,
        IWebHostEnvironment environment)
    {
        _employeeService = employeeService;
        _departmentService = departmentService;
        _positionService = positionService;
        _fileStorageService = fileStorageService;
        _reportingService = reportingService;
        _environment = environment;
    }

    // =========================================================
    // INDEX
    // =========================================================
    [HttpGet]
    [Authorize(Roles = "Administrator,HR Manager,Manager")]
    public async Task<IActionResult> Index(
        EmployeeSearchModel searchModel)
    {
        var result =
            await _employeeService.GetPagedAsync(searchModel);

        var departments =
            await _departmentService.GetLookupAsync();

        var positions =
            await _positionService.GetLookupAsync();

        var model = new EmployeeIndexViewModel
        {
            Employees = result,

            Search = searchModel.Search,

            DepartmentId = searchModel.DepartmentId,

            PositionId = searchModel.PositionId,

            EmploymentType = searchModel.EmploymentType,

            Status = searchModel.Status,

            HireDateFrom = searchModel.HireDateFrom,

            HireDateTo = searchModel.HireDateTo,

            PageSize = searchModel.PageSize,

            SortBy = searchModel.SortBy,

            SortDescending = searchModel.SortDescending,

            Departments = departments,

            Positions = positions
        };

        return View(model);
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

    [Authorize(Roles = "Administrator,HR Manager")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadEmployeeFormLookups();

        var model = new EmployeeCreateModel
        {
            HireDate = DateTime.Today,
            Status = EmployeeStatus.Active,
            EmploymentType = EmploymentType.Regular
        };

        return View(model);
    }

    // =========================================================
    // CREATE - POST
    // =========================================================

 
    [Authorize(Roles = "Administrator,HR Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadEmployeeFormLookups();

            return View(model);
        }

        var employeeId = await _employeeService.CreateAsync(model);

        return RedirectToAction(
            nameof(Details),
            new { id = employeeId });
    }
    // =========================================================
    // EDIT - GET
    // =========================================================

    [Authorize(Roles = "Administrator,HR Manager")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);

        if (employee == null)
            return NotFound();

        await LoadEmployeeFormLookups();

        var model = new EmployeeEditViewModel
        {
            Employee = new EmployeeEditModel
            {
                EmployeeNumber = employee.EmployeeNumber,
                FirstName = employee.FirstName,
                MiddleName = employee.MiddleName,
                LastName = employee.LastName,
                Suffix = employee.Suffix,
                BirthDate = employee.BirthDate,
                Gender = employee.Gender,
                CivilStatus = employee.CivilStatus,
                Email = employee.Email,
                PhoneNumber = employee.PhoneNumber,
                Address = employee.Address,
                HireDate = employee.HireDate,
                RegularizationDate = employee.RegularizationDate,
                EmploymentType = employee.EmploymentType,
                Status = employee.Status,
                BasicSalary = employee.BasicSalary,
                DepartmentId = employee.DepartmentId,
                PositionId = employee.PositionId,
                ProfileImage = employee.ProfileImage
            }
        };

        ViewBag.EmployeeId = id;

        return View(model);
    }

    // =========================================================
    // EDIT - POST
    // =========================================================


    [Authorize(Roles = "Administrator,HR Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
    int id,
    EmployeeEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadEmployeeFormLookups();

            ViewBag.EmployeeId = id;

            return View(model);
        }

        var employee = await _employeeService.GetByIdAsync(id);

        if (employee == null)
            return NotFound();

        // Keep existing image if no new image was selected.
        var profileImagePath = employee.ProfileImage;

        if (model.ProfileImageFile != null &&
            model.ProfileImageFile.Length > 0)
        {
            profileImagePath = await SaveProfileImageAsync(
                model.ProfileImageFile,
                employee.EmployeeNumber);
        }

        model.Employee.ProfileImage = profileImagePath;

        var updated = await _employeeService.UpdateAsync(
            id,
            model.Employee);

        if (!updated)
            return NotFound();

        return RedirectToAction(
            nameof(Details),
            new { id });
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

        var file = await _reportingService
           .GenerateEmployeeMasterListAsync(reportRequest, "PDF");

        return File(
            file,
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

    private async Task LoadEmployeeFormLookups()
    {
        ViewBag.Departments =
            await _departmentService.GetLookupAsync();

        ViewBag.Positions =
            await _positionService.GetLookupAsync();
    }

    private async Task<string> SaveProfileImageAsync(
    IFormFile file,
    string employeeNumber)
    {
        var uploadsFolder = Path.Combine(
            _environment.WebRootPath,
            "uploads",
            "employees");

        Directory.CreateDirectory(uploadsFolder);

        var extension = Path.GetExtension(file.FileName)
            .ToLowerInvariant();

        var allowedExtensions = new[]
        {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

        if (!allowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                "Only JPG, JPEG, PNG, and WEBP images are allowed.");
        }

        if (file.Length > 5 * 1024 * 1024)
        {
            throw new InvalidOperationException(
                "Profile image must not exceed 5 MB.");
        }

        var safeEmployeeNumber = string.Join(
            "_",
            employeeNumber.Split(
                Path.GetInvalidFileNameChars(),
                StringSplitOptions.RemoveEmptyEntries));

        var fileName =
            $"{safeEmployeeNumber}_{Guid.NewGuid():N}{extension}";

        var filePath = Path.Combine(
            uploadsFolder,
            fileName);

        await using var stream =
            new FileStream(
                filePath,
                FileMode.Create);

        await file.CopyToAsync(stream);

        return $"/uploads/employees/{fileName}";
    }

    [Authorize(Roles = "Administrator,HR Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _employeeService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] =
            "Employee was deleted successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Deleted()
    {
        var employees =
            await _employeeService.GetDeletedAsync();

        return View(employees);
    }

    [Authorize(Roles = "Administrator,HR Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id)
    {
        var restored =
            await _employeeService.RestoreAsync(id);

        if (!restored)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] =
            "Employee was restored successfully.";

        return RedirectToAction(nameof(Deleted));
    }

    [Authorize(Roles = "Administrator,HR Manager")]
    [HttpGet]
    public async Task<IActionResult> DeletedDetails(int id)
    {
        var employee = await _employeeService.GetDeletedByIdAsync(id);

        if (employee == null)
        {
            return NotFound();
        }

        return View(employee);
    }
    [Authorize(Roles = "Administrator,HR Manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PermanentlyDelete(int id)
    {
        var deleted = await _employeeService.PermanentlyDeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] =
            "Employee was permanently deleted.";

        return RedirectToAction(nameof(Deleted));
    }

    //[HttpGet]
    //public IActionResult TestError()
    //{
    //    throw new InvalidOperationException(
    //        "This is a test exception.");
    //}
}