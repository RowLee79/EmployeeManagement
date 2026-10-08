using EmployeeManagement.Application.Common.Interfaces;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace EmployeeManagement.IntegrationTests;
internal static class TestContext
{
    public static EmployeeDbContext Create(string? name = null) => new(
        new DbContextOptionsBuilder<EmployeeDbContext>().UseInMemoryDatabase(name ?? Guid.NewGuid().ToString()).Options,
        new TestUser(), new TestClock());
    public static async Task<Employee> AddEmployeeAsync(EmployeeDbContext db)
    {
        var department = new Department { Code = "IT", Name = "IT" };
        var position = new Position { Code = "DEV", Name = "Developer", Department = department };
        var employee = new Employee { EmployeeNumber = "EMP-001", FirstName = "Test", LastName = "Employee",
            Email = "test@example.com", Department = department, Position = position, Status = EmployeeStatus.Active,
            BirthDate = new DateTime(1990,1,1), HireDate = new DateTime(2020,1,1), BasicSalary = 30000 };
        db.Employees.Add(employee); await db.SaveChangesAsync(); return employee;
    }
}
internal sealed class TestUser : ICurrentUserService
{
    public string? UserId => "test"; public string? UserName => "tester"; public string? Email => "tester@example.com";
    public bool IsAuthenticated => true; public IReadOnlyList<string> Roles => ["Administrator"];
    public bool IsInRole(string role) => Roles.Contains(role);
}
internal sealed class TestClock : IDateTimeService { public DateTime UtcNow => new(2026, 10, 8, 0, 0, 0, DateTimeKind.Utc); }
