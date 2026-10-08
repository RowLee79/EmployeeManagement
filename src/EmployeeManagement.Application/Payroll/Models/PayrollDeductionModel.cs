namespace EmployeeManagement.Application.Payroll.Models;

public class PayrollDeductionModel
{
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
}