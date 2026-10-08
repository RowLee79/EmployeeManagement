using EmployeeManagement.Application.Payroll.Interfaces;
using EmployeeManagement.Application.Payroll.Models;

namespace EmployeeManagement.Application.Payroll.Services;

public class PayrollCalculationEngine
    : IPayrollCalculationEngine
{
    private readonly IPayrollDeductionCalculator
        _deductionCalculator;

    public PayrollCalculationEngine(
        IPayrollDeductionCalculator deductionCalculator)
    {
        _deductionCalculator = deductionCalculator;
    }

    public PayrollCalculationResult Calculate(
        PayrollCalculationInput input)
    {
        Validate(input);

        var grossSalary =
            input.BasicSalary +
            input.Overtime +
            input.Allowances;

        var deductionResult =
            _deductionCalculator.Calculate(
                grossSalary);

        deductionResult.Deductions.OtherDeductions =
            input.OtherDeductions;

        return new PayrollCalculationResult
        {
            BasicSalary = input.BasicSalary,
            Overtime = input.Overtime,
            Allowances = input.Allowances,
            GrossSalary = grossSalary,
            Deductions = deductionResult.Deductions
        };
    }

    private static void Validate(
        PayrollCalculationInput input)
    {
        if (input.BasicSalary < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(input.BasicSalary),
                "Basic salary cannot be negative.");
        }

        if (input.Overtime < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(input.Overtime),
                "Overtime cannot be negative.");
        }

        if (input.Allowances < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(input.Allowances),
                "Allowances cannot be negative.");
        }

        if (input.OtherDeductions < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(input.OtherDeductions),
                "Other deductions cannot be negative.");
        }
    }
}