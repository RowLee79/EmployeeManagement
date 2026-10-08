using EmployeeManagement.Application.Dashboard.Models;

namespace EmployeeManagement.Application.Dashboard.Interfaces;

public interface IDashboardService
{
    Task<DashboardModel> GetDashboardAsync();
}