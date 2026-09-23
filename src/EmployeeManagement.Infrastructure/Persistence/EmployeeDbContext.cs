using EmployeeManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Persistence;

public class EmployeeDbContext : DbContext
{
    public EmployeeDbContext(
        DbContextOptions<EmployeeDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees =>
        Set<Employee>();

    public DbSet<Department> Departments =>
        Set<Department>();

    public DbSet<Position> Positions =>
        Set<Position>();

    public DbSet<Attendance> Attendances =>
        Set<Attendance>();

    public DbSet<LeaveRequest> LeaveRequests =>
        Set<LeaveRequest>();

    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();

    public DbSet<Payroll> Payrolls =>
        Set<Payroll>();


    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Employee>()
            .HasIndex(x => x.EmployeeNumber)
            .IsUnique();

        modelBuilder.Entity<Employee>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<Department>()
            .HasIndex(x => x.Code)
            .IsUnique();

        modelBuilder.Entity<Position>()
            .HasIndex(x => x.Code)
            .IsUnique();

        modelBuilder.Entity<Employee>()
            .HasOne(x => x.Department)
            .WithMany(x => x.Employees)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Employee>()
            .HasOne(x => x.Position)
            .WithMany(x => x.Employees)
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Position>()
            .HasOne(x => x.Department)
            .WithMany(x => x.Positions)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Attendance>()
            .HasOne(x => x.Employee)
            .WithMany(x => x.Attendances)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LeaveRequest>()
            .HasOne(x => x.Employee)
            .WithMany(x => x.LeaveRequests)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Payroll>()
            .HasOne(x => x.Employee)
            .WithMany(x => x.Payrolls)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Attendance>()
            .HasIndex(x =>
                new
                {
                    x.EmployeeId,
                    x.AttendanceDate
                })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        modelBuilder.Entity<LeaveRequest>()
            .HasOne(x => x.Employee)
            .WithMany(x => x.LeaveRequests)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LeaveBalance>()
            .HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LeaveBalance>()
            .HasIndex(x => new
            {
                x.EmployeeId,
                x.Year,
                x.LeaveType
            })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        modelBuilder.Entity<Employee>()
            .HasIndex(x => x.HireDate);

        modelBuilder.Entity<Employee>()
            .HasIndex(x => x.Status);

        modelBuilder.Entity<Employee>()
            .HasIndex(x => x.EmploymentType);

        modelBuilder.Entity<Employee>()
            .HasIndex(x => x.DepartmentId);

        modelBuilder.Entity<Employee>()
            .HasIndex(x => x.PositionId);
    }
}