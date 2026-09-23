using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        EmployeeDbContext context)
    {
        // Apply pending migrations.
        await context.Database.MigrateAsync();

        // Seed Departments.
        await SeedDepartmentsAsync(context);

        // Seed Positions.
        await SeedPositionsAsync(context);

        // Seed Employees.
        await SeedEmployeesAsync(context);

        // Seed Leave Balances.
        await SeedLeaveBalancesAsync(context);
    }

    // ============================================================
    // Departments
    // ============================================================

    private static async Task SeedDepartmentsAsync(
        EmployeeDbContext context)
    {
        if (await context.Departments.AnyAsync())
            return;

        var departments = new List<Department>
        {
            new()
            {
                Code = "IT",
                Name = "Information Technology",
                Description = "Information Technology Department",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "Seeder"
            },

            new()
            {
                Code = "HR",
                Name = "Human Resources",
                Description = "Human Resources Department",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "Seeder"
            },

            new()
            {
                Code = "FIN",
                Name = "Finance",
                Description = "Finance Department",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "Seeder"
            },

            new()
            {
                Code = "OPS",
                Name = "Operations",
                Description = "Operations Department",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "Seeder"
            }
        };

        await context.Departments.AddRangeAsync(departments);

        await context.SaveChangesAsync();
    }

    // ============================================================
    // Positions
    // ============================================================

    private static async Task SeedPositionsAsync(
        EmployeeDbContext context)
    {
        if (await context.Positions.AnyAsync())
            return;

        var itDepartment = await context.Departments
            .FirstAsync(x => x.Code == "IT");

        var hrDepartment = await context.Departments
            .FirstAsync(x => x.Code == "HR");

        var positions = new List<Position>
        {
            new()
            {
                Code = "DEV",
                Name = "Software Developer",
                Description = "Develops and maintains software applications.",
                MinimumSalary = 30000,
                MaximumSalary = 60000,
                DepartmentId = itDepartment.Id,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "Seeder"
            },

            new()
            {
                Code = "SDEV",
                Name = "Senior Software Developer",
                Description = "Senior developer responsible for designing and developing enterprise applications.",
                MinimumSalary = 60000,
                MaximumSalary = 120000,
                DepartmentId = itDepartment.Id,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "Seeder"
            },

            new()
            {
                Code = "HR-OFF",
                Name = "HR Officer",
                Description = "Handles employee and human resources activities.",
                MinimumSalary = 30000,
                MaximumSalary = 60000,
                DepartmentId = hrDepartment.Id,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "Seeder"
            }
        };

        await context.Positions.AddRangeAsync(positions);

        await context.SaveChangesAsync();
    }

    // ============================================================
    // Employees
    // ============================================================

    private static async Task SeedEmployeesAsync(
        EmployeeDbContext context)
    {
        if (await context.Employees.AnyAsync())
            return;

        var itDepartment = await context.Departments
            .FirstAsync(x => x.Code == "IT");

        var hrDepartment = await context.Departments
            .FirstAsync(x => x.Code == "HR");

        var developerPosition = await context.Positions
            .FirstAsync(x => x.Code == "DEV");

        var seniorDeveloperPosition = await context.Positions
            .FirstAsync(x => x.Code == "SDEV");

        var hrOfficerPosition = await context.Positions
            .FirstAsync(x => x.Code == "HR-OFF");

        var employees = new List<Employee>
        {
            new()
            {
                EmployeeNumber = "EMP-000001",

                FirstName = "Juan",
                MiddleName = "Dela",
                LastName = "Cruz",

                BirthDate = new DateTime(1992, 5, 15),

                Gender = Gender.Male,

                CivilStatus = "Single",

                Email = "juan.cruz@example.com",

                PhoneNumber = "09171234567",

                Address = "Manila, Philippines",

                HireDate = new DateTime(2022, 1, 10),

                RegularizationDate =
                    new DateTime(2022, 7, 10),

                EmploymentType =
                    EmploymentType.Regular,

                Status =
                    EmployeeStatus.Active,

                BasicSalary = 45000,

                DepartmentId = itDepartment.Id,

                PositionId = developerPosition.Id,

                CreatedDate = DateTime.UtcNow,

                CreatedBy = "Seeder"
            },

            new()
            {
                EmployeeNumber = "EMP-000002",

                FirstName = "Maria",
                MiddleName = "Santos",
                LastName = "Garcia",

                BirthDate = new DateTime(1990, 8, 20),

                Gender = Gender.Female,

                CivilStatus = "Married",

                Email = "maria.garcia@example.com",

                PhoneNumber = "09181234567",

                Address = "Quezon City, Philippines",

                HireDate = new DateTime(2021, 3, 15),

                RegularizationDate =
                    new DateTime(2021, 9, 15),

                EmploymentType =
                    EmploymentType.Regular,

                Status =
                    EmployeeStatus.Active,

                BasicSalary = 75000,

                DepartmentId = itDepartment.Id,

                PositionId = seniorDeveloperPosition.Id,

                CreatedDate = DateTime.UtcNow,

                CreatedBy = "Seeder"
            },

            new()
            {
                EmployeeNumber = "EMP-000003",

                FirstName = "Ana",
                MiddleName = "Reyes",
                LastName = "Santos",

                BirthDate = new DateTime(1995, 11, 3),

                Gender = Gender.Female,

                CivilStatus = "Single",

                Email = "ana.santos@example.com",

                PhoneNumber = "09191234567",

                Address = "Makati City, Philippines",

                HireDate = new DateTime(2023, 6, 1),

                RegularizationDate =
                    new DateTime(2023, 12, 1),

                EmploymentType =
                    EmploymentType.Regular,

                Status =
                    EmployeeStatus.Active,

                BasicSalary = 40000,

                DepartmentId = hrDepartment.Id,

                PositionId = hrOfficerPosition.Id,

                CreatedDate = DateTime.UtcNow,

                CreatedBy = "Seeder"
            }
        };

        await context.Employees.AddRangeAsync(employees);

        await context.SaveChangesAsync();
    }

    // ============================================================
    // Leave Balances
    // ============================================================

    private static async Task SeedLeaveBalancesAsync(
        EmployeeDbContext context)
    {
        var employees = await context.Employees
            .Where(x => !x.IsDeleted)
            .ToListAsync();

        if (!employees.Any())
            return;

        var year = DateTime.UtcNow.Year;

        foreach (var employee in employees)
        {
            var existingBalances =
                await context.LeaveBalances
                    .Where(x =>
                        x.EmployeeId == employee.Id &&
                        x.Year == year &&
                        !x.IsDeleted)
                    .Select(x => x.LeaveType)
                    .ToListAsync();

            var balances = new List<LeaveBalance>();

            // ----------------------------------------------------
            // Vacation Leave
            // ----------------------------------------------------

            if (!existingBalances.Contains(
                    LeaveType.Vacation))
            {
                balances.Add(
                    new LeaveBalance
                    {
                        EmployeeId = employee.Id,

                        Year = year,

                        LeaveType =
                            LeaveType.Vacation,

                        AllocatedDays = 15,

                        UsedDays = 0,

                        CreatedDate =
                            DateTime.UtcNow,

                        CreatedBy = "Seeder"
                    });
            }

            // ----------------------------------------------------
            // Sick Leave
            // ----------------------------------------------------

            if (!existingBalances.Contains(
                    LeaveType.Sick))
            {
                balances.Add(
                    new LeaveBalance
                    {
                        EmployeeId = employee.Id,

                        Year = year,

                        LeaveType =
                            LeaveType.Sick,

                        AllocatedDays = 15,

                        UsedDays = 0,

                        CreatedDate =
                            DateTime.UtcNow,

                        CreatedBy = "Seeder"
                    });
            }

            // ----------------------------------------------------
            // Emergency Leave
            // ----------------------------------------------------

            if (!existingBalances.Contains(
                    LeaveType.Emergency))
            {
                balances.Add(
                    new LeaveBalance
                    {
                        EmployeeId = employee.Id,

                        Year = year,

                        LeaveType =
                            LeaveType.Emergency,

                        AllocatedDays = 5,

                        UsedDays = 0,

                        CreatedDate =
                            DateTime.UtcNow,

                        CreatedBy = "Seeder"
                    });
            }

            if (balances.Count > 0)
            {
                await context.LeaveBalances
                    .AddRangeAsync(balances);
            }
        }

        await context.SaveChangesAsync();
    }
}