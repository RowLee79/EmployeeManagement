using EmployeeManagement.Application.Payroll.Models;

namespace EmployeeManagement.Application.Payroll.Interfaces;

public interface IPayrollTaxCalculator
{
    PayrollTaxCalculationResult Calculate(
        decimal taxableIncome);
}