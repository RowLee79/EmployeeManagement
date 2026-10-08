using EmployeeManagement.Application.Payroll.Interfaces;
using EmployeeManagement.Application.Payroll.Models;
using Microsoft.Extensions.Options;

namespace EmployeeManagement.Infrastructure.Services;

public class PayrollTaxCalculator
    : IPayrollTaxCalculator
{
    private readonly IReadOnlyList<PayrollTaxBracket>
        _brackets;

    public PayrollTaxCalculator(
        IOptions<List<PayrollTaxBracket>> options)
    {
        _brackets =
            options.Value
                .OrderBy(x => x.MinimumIncome)
                .ToList();
    }

    public PayrollTaxCalculationResult Calculate(
        decimal taxableIncome)
    {
        if (taxableIncome < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(taxableIncome),
                "Taxable income cannot be negative.");
        }

        if (_brackets.Count == 0)
        {
            return new PayrollTaxCalculationResult
            {
                TaxableIncome = taxableIncome
            };
        }

        var bracket =
            FindBracket(taxableIncome);

        if (bracket == null)
        {
            return new PayrollTaxCalculationResult
            {
                TaxableIncome = taxableIncome
            };
        }

        var taxableExcess =
            Math.Max(
                0,
                taxableIncome -
                bracket.ExcessOver);

        var tax =
            bracket.BaseTax +
            taxableExcess *
            bracket.TaxRate;

        tax =
            RoundMoney(tax);

        return new PayrollTaxCalculationResult
        {
            TaxableIncome =
                taxableIncome,

            BaseTax =
                bracket.BaseTax,

            TaxableExcess =
                taxableExcess,

            TaxRate =
                bracket.TaxRate,

            WithholdingTax =
                tax
        };
    }

    private PayrollTaxBracket? FindBracket(
        decimal taxableIncome)
    {
        return _brackets
            .Where(x =>
                taxableIncome >=
                x.MinimumIncome)
            .Where(x =>
                !x.MaximumIncome.HasValue ||
                taxableIncome <=
                x.MaximumIncome.Value)
            .OrderByDescending(
                x => x.MinimumIncome)
            .FirstOrDefault();
    }

    private static decimal RoundMoney(
        decimal value)
    {
        return Math.Round(
            value,
            2,
            MidpointRounding.AwayFromZero);
    }
}