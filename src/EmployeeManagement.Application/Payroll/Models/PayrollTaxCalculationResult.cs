namespace EmployeeManagement.Application.Payroll.Models;

public class PayrollTaxCalculationResult
{
    public decimal TaxableIncome { get; set; }

    public decimal BaseTax { get; set; }

    public decimal TaxableExcess { get; set; }

    public decimal TaxRate { get; set; }

    public decimal WithholdingTax { get; set; }
}