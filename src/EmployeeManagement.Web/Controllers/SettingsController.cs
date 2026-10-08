using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace EmployeeManagement.Web.Controllers;
[Authorize(Roles = "Administrator")]
public sealed class SettingsController(IConfiguration configuration) : Controller
{
    public IActionResult Index()
    {
        ViewBag.ReportingProvider = configuration["Reporting:Provider"] ?? "Native";
        ViewBag.DemoData = configuration.GetValue<bool>("Database:SeedDemoData");
        ViewBag.TaxEnabled = configuration.GetValue<bool>("Payroll:Deductions:CalculateWithholdingTax");
        return View();
    }
}
