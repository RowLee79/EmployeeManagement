using EmployeeManagement.Application.Payroll.Models;

namespace EmployeeManagement.Web.Services.Payroll;

public interface IPayslipPdfService
{
    byte[] GeneratePayslip(PayslipModel payslip);
}