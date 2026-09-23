using EmployeeManagement.Application.Employees.Interfaces;
using EmployeeManagement.Application.Employees.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Web.Controllers;

[Authorize(Roles = "Administrator,HR Manager,Manager")]
public class ReportsController : Controller
{
    private readonly IEmployeeService _employeeService;

    public ReportsController(
        IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public IActionResult Employees()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> EmployeeData(
        string? search,
        int? departmentId,
        int? positionId,
        EmployeeManagement.Domain.Enums.EmployeeStatus? status,
        EmployeeManagement.Domain.Enums.EmploymentType? employmentType,
        DateTime? hireDateFrom,
        DateTime? hireDateTo)
    {
        var model = new EmployeeSearchModel
        {
            Search = search,
            DepartmentId = departmentId,
            PositionId = positionId,
            Status = status,
            EmploymentType = employmentType,
            HireDateFrom = hireDateFrom,
            HireDateTo = hireDateTo
        };

        var employees =
            await _employeeService.GetEmployeeReportAsync(model);

        return Json(employees);
    }
}