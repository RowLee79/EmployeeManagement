using EmployeeManagement.Application.Payroll.Models;

namespace EmployeeManagement.Application.Payroll.Interfaces;

public interface IPayrollPeriodService
{
    Task<IReadOnlyList<PayrollPeriodListModel>> GetAllAsync();

    Task<PayrollPeriodDetailsModel?> GetByIdAsync(
        int id);

    Task<int> CreateAsync(
        PayrollPeriodCreateModel model);

    Task<bool> UpdateAsync(
        int id,
        PayrollPeriodCreateModel model);

    Task<bool> DeleteAsync(
        int id);

    Task<bool> StartProcessingAsync(
        int id);

    Task<bool> ApproveAsync(
        int id);

    Task<bool> FinalizeAsync(
        int id);

    Task<bool> CloseAsync(
        int id);

    Task<bool> LockAsync(
        int id);

    Task<IReadOnlyList<PayrollPeriodLookup>>
    GetOpenLookupAsync();
}