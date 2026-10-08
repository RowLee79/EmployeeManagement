using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Web.Controllers;

[Microsoft.AspNetCore.Authorization.AllowAnonymous]
public class ErrorController : Controller
{
    [Route("Error")]
    public IActionResult Index(string? correlationId)
    {
        ViewBag.CorrelationId = correlationId;

        return View();
    }
}