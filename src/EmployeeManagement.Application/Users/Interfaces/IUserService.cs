using EmployeeManagement.Application.Users.Models;

namespace EmployeeManagement.Application.Users.Interfaces;

public interface IUserService
{
    Task<IReadOnlyList<UserListModel>> GetAllAsync();

    Task<UserDetailsModel?> GetByIdAsync(string id);

    Task<string> CreateAsync(
        UserCreateModel model);

    Task<bool> UpdateAsync(
        UserEditModel model);

    Task<bool> SetActiveAsync(
        string id,
        bool isActive);

    Task<bool> ResetPasswordAsync(
        string id,
        string newPassword);

    Task<bool> DeleteAsync(
        string id);

    Task<IReadOnlyList<string>> GetRolesAsync();

    Task<IReadOnlyList<EmployeeLookupModel>>
        GetEmployeeLookupAsync();
}