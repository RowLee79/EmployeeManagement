using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
namespace EmployeeManagement.Web.Filters;
public sealed class ConcurrencyExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var duplicate = context.Exception is DbUpdateException { InnerException: SqlException sql } &&
            sql.Number is 2601 or 2627;
        if (context.Exception is not DbUpdateConcurrencyException && !duplicate) return;
        context.Result = new ObjectResult(new ProblemDetails
        {
            Status = 409, Title = duplicate ? "A matching record already exists." : "This record changed while you were editing it.",
            Detail = "Reload the page and review the current values before retrying."
        }) { StatusCode = 409 };
        context.ExceptionHandled = true;
    }
}
