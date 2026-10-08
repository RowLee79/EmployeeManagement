using EmployeeManagement.Application.LeaveManagement.Interfaces;
using EmployeeManagement.Application.LeaveManagement.Models;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace EmployeeManagement.Infrastructure.Services;
public sealed class LeaveAllocationService(EmployeeDbContext context) : ILeaveAllocationService
{
    public async Task SetAllocationAsync(LeaveAllocationModel model)
    {
        if (model.Year is < 2000 or > 2100 || model.AllocatedDays is < 0 or > 366 || !Enum.IsDefined(model.LeaveType))
            throw new InvalidOperationException("Enter a valid year, leave type and allocation.");
        if (!await context.Employees.AnyAsync(x => x.Id == model.EmployeeId && !x.IsDeleted))
            throw new InvalidOperationException("Select an existing employee.");
        var balance = await context.LeaveBalances.FirstOrDefaultAsync(x => x.EmployeeId == model.EmployeeId &&
            x.Year == model.Year && x.LeaveType == model.LeaveType && !x.IsDeleted);
        if (balance is null)
            context.LeaveBalances.Add(new LeaveBalance { EmployeeId = model.EmployeeId, Year = model.Year,
                LeaveType = model.LeaveType, AllocatedDays = model.AllocatedDays });
        else
        {
            if (model.AllocatedDays < balance.UsedDays)
                throw new InvalidOperationException($"Allocation cannot be below {balance.UsedDays:0.##} used days.");
            balance.AllocatedDays = model.AllocatedDays;
        }
        await context.SaveChangesAsync();
    }
}
