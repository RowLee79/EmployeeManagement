using EmployeeManagement.Application.Common.Models;
using EmployeeManagement.Application.Departments.Models;
using EmployeeManagement.Application.Positions.Models;
using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Application.Employees.Models;

public class EmployeeIndexViewModel
{
    // ============================================================
    // RESULTS
    // ============================================================

    public PagedResult<EmployeeListModel> Employees { get; set; }
        = null!;


    // ============================================================
    // FILTERS
    // ============================================================

    public string? Search { get; set; }

    public int? DepartmentId { get; set; }

    public int? PositionId { get; set; }

    public EmploymentType? EmploymentType { get; set; }

    public EmployeeStatus? Status { get; set; }

    public DateTime? HireDateFrom { get; set; }

    public DateTime? HireDateTo { get; set; }


    // ============================================================
    // PAGING
    // ============================================================

    public int PageSize { get; set; } = 10;


    // ============================================================
    // SORTING
    // ============================================================

    public string SortBy { get; set; } = "HireDate";

    public bool SortDescending { get; set; } = true;


    // ============================================================
    // LOOKUPS
    // ============================================================

    public IReadOnlyList<DepartmentLookup> Departments { get; set; }
        = [];

    public IReadOnlyList<PositionLookup> Positions { get; set; }
        = [];
}