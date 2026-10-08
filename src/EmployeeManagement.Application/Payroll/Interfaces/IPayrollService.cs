using EmployeeManagement.Application.Common.Models;
using EmployeeManagement.Application.Payroll.Models;

namespace EmployeeManagement.Application.Payroll.Interfaces;

public interface IPayrollService
{
    // =========================================================
    // PAYROLL QUERY
    // =========================================================

    Task<PagedResult<PayrollListModel>> GetPagedAsync(
        PayrollSearchModel searchModel);

    Task<PayrollDetailsModel?> GetByIdAsync(
        int id);

    Task<IReadOnlyList<PayrollEmployeeLookup>>
        GetEmployeeLookupAsync();

    Task<PayrollDashboardModel>
        GetDashboardAsync();

    Task<IReadOnlyList<PayrollListModel>>
        GetEmployeeHistoryAsync(
            int employeeId);

    Task<PayslipModel?>
        GetPayslipAsync(
            int payrollId);


    // =========================================================
    // PAYROLL PERIOD LOOKUP
    // =========================================================

    Task<IReadOnlyList<PayrollPeriodLookup>>
        GetPayrollPeriodLookupAsync();


    // =========================================================
    // PAYROLL CRUD
    // =========================================================

    Task<int> CreateAsync(
        PayrollCreateModel model);

    Task<bool> UpdateAsync(
        int id,
        PayrollCreateModel model);

    Task<bool> DeleteAsync(
        int id);


    // =========================================================
    // INDIVIDUAL PAYROLL WORKFLOW
    // =========================================================

    Task<bool> CalculateAsync(
        int id);

    Task<bool> ApproveAsync(
        int id);

    Task<bool> FinalizeAsync(
        int id);

    Task<bool> CancelAsync(
        int id);


    // =========================================================
    // PAYROLL PERIOD
    // =========================================================

    Task<IReadOnlyList<PayrollListModel>>
        GetByPayrollPeriodAsync(
            int payrollPeriodId);


    // =========================================================
    // PAYROLL GENERATION
    // =========================================================

    Task<PayrollGenerationResult>
        GeneratePayrollAsync(
            int payrollPeriodId);


    // =========================================================
    // BULK PAYROLL PROCESSING
    // =========================================================

    Task<int>
        CalculateAllAsync(
            int payrollPeriodId);

    Task<int>
        ApproveAllAsync(
            int payrollPeriodId);

    Task<int>
        FinalizeAllAsync(
            int payrollPeriodId);

}