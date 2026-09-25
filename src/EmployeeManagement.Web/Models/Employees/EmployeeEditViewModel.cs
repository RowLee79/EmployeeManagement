using EmployeeManagement.Application.Employees.Models;
using Microsoft.AspNetCore.Http;

namespace EmployeeManagement.Web.Models.Employees;

public class EmployeeEditViewModel
{
    public EmployeeEditModel Employee { get; set; } = new();

    public IFormFile? ProfileImageFile { get; set; }
}