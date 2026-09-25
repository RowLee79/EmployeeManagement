using EmployeeManagement.Application.Common.Authorization;
using EmployeeManagement.Application.Departments.Interfaces;
using EmployeeManagement.Application.Employees.Interfaces;
using EmployeeManagement.Application.Positions.Interfaces;
using EmployeeManagement.Web.Models.Reports;
using EmployeeManagement.Web.Services.Reporting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.Application.Reporting;

namespace EmployeeManagement.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.CanViewReports)]
public class ReportsController : Controller
{
    private readonly IEmployeeService _employeeService;
    private readonly IDepartmentService _departmentService;
    private readonly IPositionService _positionService;
    private readonly IReportingService _reportingService;

    public ReportsController(
        IEmployeeService employeeService,
        IDepartmentService departmentService,
        IPositionService positionService,
        IReportingService reportingService)
    {
        _employeeService = employeeService;
        _departmentService = departmentService;
        _positionService = positionService;
        _reportingService = reportingService;
    }

    [HttpGet]
    public async Task<IActionResult> Employees()
    {
        var departments =
            await _departmentService.GetLookupAsync();

        var positions =
            await _positionService.GetLookupAsync();

        var model = new EmployeeReportsViewModel
        {
            Departments = departments,
            Positions = positions
        };

        return View(model);
    }

    // ============================================================
    // PDF
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmployeeMasterListPdf(
        EmployeeReportRequest request)
    {
        var file =
            await _reportingService
                .GenerateEmployeeMasterListAsync(
                    request,
                    "PDF");

        return File(
            file,
            "application/pdf",
            "EmployeeMasterList.pdf");
    }

    // ============================================================
    // EXCEL
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmployeeMasterListExcel(
        EmployeeReportRequest request)
    {
        var file =
            await _reportingService
                .GenerateEmployeeMasterListAsync(
                    request,
                    "EXCEL");

        return File(
            file,
            "application/vnd.ms-excel",
            "EmployeeMasterList.xls");
    }

    // ============================================================
    // PREVIEW
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmployeeMasterListPreview(
        EmployeeReportRequest request)
    {
        var file =
            await _reportingService
                .GenerateEmployeeMasterListAsync(
                    request,
                    "PDF");

        return File(
            file,
            "application/pdf");
    }

    // ============================================================
    // PRINT
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmployeeMasterListPrint(
        EmployeeReportRequest request)
    {
        var file =
            await _reportingService
                .GenerateEmployeeMasterListAsync(
                    request,
                    "PDF");

        return File(
            file,
            "application/pdf");
    }
}