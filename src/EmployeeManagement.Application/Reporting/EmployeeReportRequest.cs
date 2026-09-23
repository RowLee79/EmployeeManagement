namespace EmployeeManagement.Application.Reporting;

public class EmployeeReportRequest
{
    public string? Search { get; set; }

    public int? DepartmentId { get; set; }

    public int? PositionId { get; set; }

    public int? EmploymentType { get; set; }

    public int? Status { get; set; }

    public DateTime? HireDateFrom { get; set; }

    public DateTime? HireDateTo { get; set; }
}