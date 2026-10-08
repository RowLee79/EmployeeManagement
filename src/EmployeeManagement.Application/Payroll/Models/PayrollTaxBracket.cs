namespace EmployeeManagement.Application.Payroll.Models;

public class PayrollTaxBracket
{
    public decimal MinimumIncome { get; set; }

    public decimal? MaximumIncome { get; set; }

    public decimal BaseTax { get; set; }

    public decimal ExcessOver { get; set; }

    public decimal TaxRate { get; set; }
}