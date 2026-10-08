using EmployeeManagement.Domain.Common;

namespace EmployeeManagement.Domain.Entities;

public class PayrollDeduction : AuditableEntity
{
    public int PayrollId { get; set; }

    public decimal Sss { get; set; }

    public decimal PhilHealth { get; set; }

    public decimal PagIbig { get; set; }

    public decimal WithholdingTax { get; set; }

    public decimal OtherDeductions { get; set; }

    public decimal TotalDeductions =>
        Sss +
        PhilHealth +
        PagIbig +
        WithholdingTax +
        OtherDeductions;

    public Payroll Payroll { get; set; } = null!;
}