using EmployeeManagement.Application.Departments.Models;
using EmployeeManagement.Application.Positions.Models;

namespace EmployeeManagement.Web.Models.Reports;

public class EmployeeReportsViewModel
{
    public IReadOnlyList<DepartmentLookup> Departments { get; set; }
        = Array.Empty<DepartmentLookup>();

    public IReadOnlyList<PositionLookup> Positions { get; set; }
        = Array.Empty<PositionLookup>();
}