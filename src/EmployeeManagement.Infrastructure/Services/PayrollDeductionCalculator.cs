using EmployeeManagement.Application.Payroll.Interfaces;
using EmployeeManagement.Application.Payroll.Models;
using Microsoft.Extensions.Options;

namespace EmployeeManagement.Infrastructure.Services;

public class PayrollDeductionCalculator
    : IPayrollDeductionCalculator
{
    private readonly PayrollDeductionSettings _settings;

    private readonly IPayrollTaxCalculator
        _taxCalculator;

    public PayrollDeductionCalculator(
        IOptions<PayrollDeductionSettings> options,
        IPayrollTaxCalculator taxCalculator)
    {
        _settings = options.Value;

        _taxCalculator =
            taxCalculator;
    }

    public PayrollCalculationResult Calculate(
      decimal grossSalary)
    {
        if (grossSalary < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(grossSalary),
                "Gross salary cannot be negative.");
        }

        var sss =
            CalculateSss(grossSalary);

        var philHealth =
            CalculatePhilHealth(grossSalary);

        var pagIbig =
            CalculatePagIbig(grossSalary);

        var withholdingTax = 0m;

        if (_settings.CalculateWithholdingTax)
        {
            var taxResult =
                _taxCalculator.Calculate(
                    grossSalary);

            withholdingTax =
                taxResult.WithholdingTax;
        }

        var deductions =
            new PayrollDeductionModel
            {
                Sss =
                    sss,

                PhilHealth =
                    philHealth,

                PagIbig =
                    pagIbig,

                WithholdingTax =
                    withholdingTax,

                OtherDeductions = 0
            };

        return new PayrollCalculationResult
        {
            GrossSalary =
                RoundMoney(grossSalary),

            Deductions =
                deductions
        };
    }

    private decimal CalculateSss(
        decimal grossSalary)
    {
        if (_settings.SssEmployeeRate <= 0)
        {
            return 0;
        }

        return RoundMoney(
            grossSalary *
            _settings.SssEmployeeRate);
    }

    private decimal CalculatePhilHealth(
        decimal grossSalary)
    {
        if (_settings.PhilHealthEmployeeRate <= 0)
        {
            return 0;
        }

        return RoundMoney(
            grossSalary *
            _settings.PhilHealthEmployeeRate);
    }

    private decimal CalculatePagIbig(
        decimal grossSalary)
    {
        if (_settings.PagIbigEmployeeRate <= 0)
        {
            return 0;
        }

        var contribution =
            grossSalary *
            _settings.PagIbigEmployeeRate;

        if (_settings.PagIbigMaximumContribution > 0)
        {
            contribution =
                Math.Min(
                    contribution,
                    _settings.PagIbigMaximumContribution);
        }

        return RoundMoney(contribution);
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