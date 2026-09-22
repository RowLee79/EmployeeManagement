using EmployeeManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        EmployeeDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!await context.Departments.AnyAsync())
        {
            var departments = new[]
            {
                new Department
                {
                    Code = "IT",
                    Name = "Information Technology",
                    Description = "Technology and software development",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Department
                {
                    Code = "HR",
                    Name = "Human Resources",
                    Description = "Human resources management",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Department
                {
                    Code = "FIN",
                    Name = "Finance",
                    Description = "Finance and accounting",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Department
                {
                    Code = "OPS",
                    Name = "Operations",
                    Description = "Business operations",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                }
            };

            await context.Departments.AddRangeAsync(
                departments);

            await context.SaveChangesAsync();
        }

        if (!await context.Positions.AnyAsync())
        {
            var it = await context.Departments
                .FirstAsync(x => x.Code == "IT");

            var hr = await context.Departments
                .FirstAsync(x => x.Code == "HR");

            var positions = new[]
            {
                new Position
                {
                    Code = "DEV",
                    Name = "Software Developer",
                    DepartmentId = it.Id,
                    MinimumSalary = 30000,
                    MaximumSalary = 100000,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Position
                {
                    Code = "SDEV",
                    Name = "Senior Software Developer",
                    DepartmentId = it.Id,
                    MinimumSalary = 70000,
                    MaximumSalary = 150000,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Position
                {
                    Code = "HR-OFF",
                    Name = "HR Officer",
                    DepartmentId = hr.Id,
                    MinimumSalary = 30000,
                    MaximumSalary = 80000,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                }
            };

            await context.Positions.AddRangeAsync(
                positions);

            await context.SaveChangesAsync();
        }
    }
}