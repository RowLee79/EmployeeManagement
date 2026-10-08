using System.ComponentModel.DataAnnotations;
using EmployeeManagement.Domain.Enums;
namespace EmployeeManagement.Application.Reporting;

public class EmployeeReportRequest : IValidatableObject
{
    public string ReportType { get; set; } = "EmployeeMasterList";

    public string? Search { get; set; }

    public int? DepartmentId { get; set; }

    public int? PositionId { get; set; }

    public int? EmploymentType { get; set; }

    public int? Status { get; set; }

    public DateTime? HireDateFrom { get; set; }

    public DateTime? HireDateTo { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (HireDateFrom > HireDateTo)
            yield return new ValidationResult("Hire date From must be before To.");
        if (EmploymentType.HasValue && !Enum.IsDefined(typeof(EmployeeManagement.Domain.Enums.EmploymentType), EmploymentType.Value))
            yield return new ValidationResult("Invalid employment type.");
        if (Status.HasValue && !Enum.IsDefined(typeof(EmployeeStatus), Status.Value))
            yield return new ValidationResult("Invalid employee status.");
    }
}