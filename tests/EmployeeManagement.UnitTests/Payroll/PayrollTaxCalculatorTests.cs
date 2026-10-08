using EmployeeManagement.Application.Payroll.Models;
using EmployeeManagement.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace EmployeeManagement.UnitTests.Payroll;

public class PayrollTaxCalculatorTests
{
    private static PayrollTaxCalculator
        CreateCalculator()
    {
        var brackets =
            new List<PayrollTaxBracket>
            {
                new()
                {
                    MinimumIncome = 0,
                    MaximumIncome = 20000,
                    BaseTax = 0,
                    ExcessOver = 0,
                    TaxRate = 0
                },

                new()
                {
                    MinimumIncome = 20000.01m,
                    MaximumIncome = 40000,
                    BaseTax = 1000,
                    ExcessOver = 20000,
                    TaxRate = 0.10m
                },

                new()
                {
                    MinimumIncome = 40000.01m,
                    MaximumIncome = 80000,
                    BaseTax = 3000,
                    ExcessOver = 40000,
                    TaxRate = 0.15m
                },

                new()
                {
                    MinimumIncome = 80000.01m,
                    MaximumIncome = null,
                    BaseTax = 9000,
                    ExcessOver = 80000,
                    TaxRate = 0.20m
                }
            };

        var options =
            Options.Create(brackets);

        return new PayrollTaxCalculator(
            options);
    }

    [Fact]
    public void Calculate_WithinFirstBracket_ReturnsZeroTax()
    {
        var calculator =
            CreateCalculator();

        var result =
            calculator.Calculate(15000);

        Assert.Equal(
            0,
            result.WithholdingTax);
    }

    [Fact]
    public void Calculate_WithinSecondBracket_ReturnsExpectedTax()
    {
        var calculator =
            CreateCalculator();

        var result =
            calculator.Calculate(30000);

        Assert.Equal(
            2000,
            result.WithholdingTax);
    }

    [Fact]
    public void Calculate_WithinThirdBracket_ReturnsExpectedTax()
    {
        var calculator =
            CreateCalculator();

        var result =
            calculator.Calculate(50000);

        Assert.Equal(
            4500,
            result.WithholdingTax);
    }

    [Fact]
    public void Calculate_WithinFourthBracket_ReturnsExpectedTax()
    {
        var calculator =
            CreateCalculator();

        var result =
            calculator.Calculate(100000);

        Assert.Equal(
            13000,
            result.WithholdingTax);
    }

    [Fact]
    public void Calculate_NegativeIncome_ThrowsException()
    {
        var calculator =
            CreateCalculator();

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                calculator.Calculate(-1));
    }
}