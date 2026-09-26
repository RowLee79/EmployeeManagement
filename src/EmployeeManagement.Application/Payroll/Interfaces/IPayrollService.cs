using EmployeeManagement.Application.Payroll.Models;

public interface IPayrollService
{
    Task<IReadOnlyList<PayrollListModel>> GetAllAsync();

    Task<PayrollDetailsModel?> GetByIdAsync(int id);

    Task<IReadOnlyList<PayrollEmployeeLookup>>
        GetEmployeeLookupAsync();

    Task<PayrollDashboardModel>
        GetDashboardAsync();

    Task<int> CreateAsync(
        PayrollCreateModel model);

    Task<bool> UpdateAsync(
        int id,
        PayrollCreateModel model);

    Task<bool> DeleteAsync(int id);

    Task<bool> CalculateAsync(int id);

    Task<bool> ApproveAsync(int id);

    Task<bool> FinalizeAsync(int id);

    Task<bool> CancelAsync(int id);
}