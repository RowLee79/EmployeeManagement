using EmployeeManagement.Application.Attendance.Models;
using EmployeeManagement.Application.LeaveManagement.Models;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Infrastructure.Services;
using EmployeeManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
namespace EmployeeManagement.IntegrationTests;
public sealed class EmployeeWorkflowTests
{
    [Fact]
    public async Task Relationships_CountEmployeesThroughActualForeignKeys()
    {
        using var db = TestContext.Create(); var employee = await TestContext.AddEmployeeAsync(db);
        Assert.Equal(1, await db.Departments.Select(x => x.Employees.Count).SingleAsync());
        Assert.Equal(1, await db.Positions.Select(x => x.Employees.Count).SingleAsync());
        Assert.Null(db.Model.FindEntityType(typeof(Employee))!.FindProperty("DepartmentId1"));
        Assert.Null(db.Model.FindEntityType(typeof(Employee))!.FindProperty("PositionId1"));
    }
    [Fact]
    public async Task Attendance_CalculatesLateAndRejectsDuplicates()
    {
        using var db = TestContext.Create(); var employee = await TestContext.AddEmployeeAsync(db);
        var service = new AttendanceService(db, new TestUser());
        var model = new AttendanceCreateModel { EmployeeId = employee.Id, AttendanceDate = new DateTime(2026,10,8),
            TimeIn = TimeSpan.FromHours(9.5), TimeOut = TimeSpan.FromHours(17.5) };
        var id = await service.CreateAsync(model);
        var attendance = await service.GetByIdAsync(id);
        Assert.Equal(30, attendance!.LateMinutes); Assert.Equal("Late", attendance.Status);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(model));
    }
    [Fact]
    public async Task Attendance_SoftDeleteAllowsReplacementAndRecordsActor()
    {
        using var db = TestContext.Create(); var employee = await TestContext.AddEmployeeAsync(db);
        var service = new AttendanceService(db, new TestUser());
        var model = new AttendanceCreateModel { EmployeeId = employee.Id, AttendanceDate = new DateTime(2026,10,8),
            Status = AttendanceStatus.Absent };
        var id = await service.CreateAsync(model); await service.DeleteAsync(id);
        Assert.Equal("tester", (await db.Attendances.FindAsync(id))!.DeletedBy);
        Assert.NotEqual(id, await service.CreateAsync(model));
    }
    [Fact]
    public async Task Attendance_RejectsMissingEmployeeOnEdit()
    {
        using var db = TestContext.Create(); var employee = await TestContext.AddEmployeeAsync(db);
        var service = new AttendanceService(db, new TestUser());
        var model = new AttendanceCreateModel { EmployeeId = employee.Id, AttendanceDate = new DateTime(2026,10,8), Status = AttendanceStatus.Absent };
        var id = await service.CreateAsync(model); model.EmployeeId = 9999;
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(id, model));
    }
    [Fact]
    public async Task Leave_AllocationApprovalAndCancellationRestoreBalance()
    {
        using var db = TestContext.Create(); var employee = await TestContext.AddEmployeeAsync(db);
        var allocations = new LeaveAllocationService(db);
        await allocations.SetAllocationAsync(new LeaveAllocationModel { EmployeeId = employee.Id, Year = 2026, LeaveType = LeaveType.Vacation, AllocatedDays = 15 });
        var leaves = new LeaveService(db, new TestUser());
        var id = await leaves.CreateAsync(new LeaveRequestCreateModel { EmployeeId = employee.Id, LeaveType = LeaveType.Vacation,
            StartDate = new DateTime(2026,10,8), EndDate = new DateTime(2026,10,9), Reason = "Vacation" });
        await leaves.ApproveAsync(id, "tester");
        Assert.Equal(2m, (await db.LeaveBalances.SingleAsync()).UsedDays);
        await Assert.ThrowsAsync<InvalidOperationException>(() => allocations.SetAllocationAsync(new LeaveAllocationModel
            { EmployeeId = employee.Id, Year = 2026, LeaveType = LeaveType.Vacation, AllocatedDays = 1 }));
        await leaves.CancelAsync(id);
        Assert.Equal(0m, (await db.LeaveBalances.SingleAsync()).UsedDays);
    }
    [Fact]
    public async Task Leave_ConcurrentBalanceUpdateIsRejected()
    {
        var name = Guid.NewGuid().ToString();
        using var first = TestContext.Create(name); using var second = TestContext.Create(name);
        var employee = await TestContext.AddEmployeeAsync(first);
        first.LeaveBalances.Add(new LeaveBalance { EmployeeId = employee.Id, Year = 2026, LeaveType = LeaveType.Sick, AllocatedDays = 15 });
        await first.SaveChangesAsync();
        var a = await first.LeaveBalances.SingleAsync(); var b = await second.LeaveBalances.SingleAsync();
        a.UsedDays = 3; await first.SaveChangesAsync(); b.UsedDays = 4;
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
    }
    [Fact]
    public void SqlServerModel_MatchesFinalMigrationSnapshot()
    {
        using var db = new EmployeeDbContext(new DbContextOptionsBuilder<EmployeeDbContext>()
            .UseSqlServer("Server=localhost;Database=ModelOnly;Integrated Security=True;TrustServerCertificate=True").Options,
            new TestUser(), new TestClock());
        Assert.False(db.Database.HasPendingModelChanges());
        var attendance = db.Model.FindEntityType(typeof(Attendance))!;
        var index = attendance.GetIndexes().Single(x => x.Properties.Count == 2);
        Assert.True(index.IsUnique); Assert.Equal("[IsDeleted] = 0", index.GetFilter());
    }
}
