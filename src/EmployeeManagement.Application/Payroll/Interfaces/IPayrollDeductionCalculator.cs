using EmployeeManagement.Application.Payroll.Models;

namespace EmployeeManagement.Application.Payroll.Interfaces;

public interface IPayrollDeductionCalculator
{
    PayrollCalculationResult Calculate(
        decimal grossSalary);
}