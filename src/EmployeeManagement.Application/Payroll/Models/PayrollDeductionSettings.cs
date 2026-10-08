namespace EmployeeManagement.Application.Payroll.Models;

public class PayrollDeductionSettings
{
    public decimal SssEmployeeRate { get; set; }

    public decimal PhilHealthEmployeeRate { get; set; }

    public decimal PagIbigEmployeeRate { get; set; }

    public decimal PagIbigMaximumContribution { get; set; }

    public bool CalculateWithholdingTax { get; set; }
}