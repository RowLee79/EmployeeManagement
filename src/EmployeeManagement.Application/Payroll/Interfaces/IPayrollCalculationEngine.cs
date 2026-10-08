using EmployeeManagement.Application.Payroll.Models;

namespace EmployeeManagement.Application.Payroll.Interfaces;

public interface IPayrollCalculationEngine
{
    PayrollCalculationResult Calculate(
        PayrollCalculationInput input);
}