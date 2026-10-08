using EmployeeManagement.Application.Payroll.Models;
using EmployeeManagement.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace EmployeeManagement.UnitTests.Payroll;

public class PayrollDeductionCalculatorTests
{
    [Fact]
    public void Calculate_WithZeroRates_ReturnsZeroDeductions()
    {
        var settings =
            Options.Create(
                new PayrollDeductionSettings
                {
                    SssEmployeeRate = 0,
                    PhilHealthEmployeeRate = 0,
                    PagIbigEmployeeRate = 0,
                    PagIbigMaximumContribution = 0,
                    CalculateWithholdingTax = false
                });

        var taxCalculator =
            new PayrollTaxCalculator(
                Options.Create(
                    new List<PayrollTaxBracket>()));

        var calculator =
            new PayrollDeductionCalculator(
                settings,
                taxCalculator);

        var result =
            calculator.Calculate(35000);

        Assert.Equal(
            35000,
            result.GrossSalary);

        Assert.Equal(
            0,
            result.Deductions.Sss);

        Assert.Equal(
            0,
            result.Deductions.PhilHealth);

        Assert.Equal(
            0,
            result.Deductions.PagIbig);

        Assert.Equal(
            0,
            result.Deductions.WithholdingTax);

        Assert.Equal(
            0,
            result.Deductions.TotalDeductions);
    }

    [Fact]
    public void Calculate_WithRates_ReturnsExpectedDeductions()
    {
        var settings =
            Options.Create(
                new PayrollDeductionSettings
                {
                    SssEmployeeRate = 0.05m,
                    PhilHealthEmployeeRate = 0.025m,
                    PagIbigEmployeeRate = 0.02m,
                    PagIbigMaximumContribution = 1000m,
                    CalculateWithholdingTax = false
                });

        var taxCalculator =
            new PayrollTaxCalculator(
                Options.Create(
                    new List<PayrollTaxBracket>()));

        var calculator =
            new PayrollDeductionCalculator(
                settings,
                taxCalculator);

        var result =
            calculator.Calculate(30000m);

        Assert.Equal(
            30000m,
            result.GrossSalary);

        Assert.Equal(
            1500m,
            result.Deductions.Sss);

        Assert.Equal(
            750m,
            result.Deductions.PhilHealth);

        Assert.Equal(
            600m,
            result.Deductions.PagIbig);

        Assert.Equal(
            0m,
            result.Deductions.WithholdingTax);

        Assert.Equal(
            2850m,
            result.Deductions.TotalDeductions);
    }

    [Fact]
    public void Calculate_WithNegativeGrossSalary_ThrowsException()
    {
        var settings =
            Options.Create(
                new PayrollDeductionSettings());

        var taxCalculator =
            new PayrollTaxCalculator(
                Options.Create(
                    new List<PayrollTaxBracket>()));

        var calculator =
            new PayrollDeductionCalculator(
                settings,
                taxCalculator);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => calculator.Calculate(-1));
    }
}