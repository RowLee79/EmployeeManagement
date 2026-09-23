using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Application.Employees.Models;

public class EmployeeSearchModel
{
    public string? Search { get; set; }

    public int? DepartmentId { get; set; }

    public int? PositionId { get; set; }

    public EmployeeStatus? Status { get; set; }

    public EmploymentType? EmploymentType { get; set; }

    public DateTime? HireDateFrom { get; set; }

    public DateTime? HireDateTo { get; set; }

    public string SortBy { get; set; } = "HireDate";

    public bool SortDescending { get; set; } = true;

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}