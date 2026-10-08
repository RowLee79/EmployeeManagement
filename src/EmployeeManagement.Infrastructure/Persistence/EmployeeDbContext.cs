using EmployeeManagement.Application.Common.Interfaces;
using EmployeeManagement.Domain.Common;
using EmployeeManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using PayrollEntity = EmployeeManagement.Domain.Entities.Payroll;
using PayrollDeductionEntity =
    EmployeeManagement.Domain.Entities.PayrollDeduction;
using PayrollPeriodEntity =
    EmployeeManagement.Domain.Entities.PayrollPeriod;
//using AttendanceEntity =
//    EmployeeManagement.Domain.Entities.Attendance;

namespace EmployeeManagement.Infrastructure.Persistence;

public class EmployeeDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;

    public EmployeeDbContext(
        DbContextOptions<EmployeeDbContext> options,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService)
        : base(options)
    {
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
    }


    // =========================================================
    // DB SETS
    // =========================================================

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<Position> Positions => Set<Position>();

    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();

    //public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    //public DbSet<LeaveAllocation> LeaveAllocations => Set<LeaveAllocation>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    //public DbSet<PayrollItem> PayrollItems => Set<PayrollItem>();
    //public DbSet<PayrollItemType> PayrollItemTypes => Set<PayrollItemType>();
    //public DbSet<PayrollPeriod> PayrollPeriods => Set<PayrollPeriod>();

    public DbSet<PayrollEntity> Payrolls => Set<PayrollEntity>();
    public DbSet<PayrollDeductionEntity> PayrollDeductions => Set<PayrollDeductionEntity>();
    public DbSet<PayrollPeriodEntity> PayrollPeriods => Set<PayrollPeriodEntity>();
    // =========================================================
    // MODEL CONFIGURATION
    // =========================================================

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Employee configuration
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.EmployeeNumber)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.MiddleName)
                .HasMaxLength(100);

            entity.Property(x => x.LastName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Suffix)
                .HasMaxLength(20);

            entity.Property(x => x.Email)
                .HasMaxLength(200);

            entity.Property(x => x.PhoneNumber)
                .HasMaxLength(50);

            entity.Property(x => x.Address)
                .HasMaxLength(500);

            entity.Property(x => x.ProfileImage)
                .HasMaxLength(500);

            entity.Property(x => x.BasicSalary)
                .HasPrecision(18, 2);


            // -------------------------------------------------
            // AUDIT FIELDS
            // -------------------------------------------------

            entity.Property(x => x.CreatedBy)
                .HasMaxLength(256);

            entity.Property(x => x.CreatedDate)
                .IsRequired();

            entity.Property(x => x.UpdatedBy)
                .HasMaxLength(256);

            entity.Property(x => x.DeletedBy)
                .HasMaxLength(256);


            // -------------------------------------------------
            // RELATIONSHIPS
            // -------------------------------------------------

            entity.HasOne(x => x.Department)
                .WithMany(x => x.Employees)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Position)
                .WithMany(x => x.Employees)
                .HasForeignKey(x => x.PositionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =====================================================
        // PAYROLL
        // =====================================================

        modelBuilder.Entity<PayrollEntity>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.BasicSalary)
                .HasPrecision(18, 2);

            entity.Property(x => x.Overtime)
                .HasPrecision(18, 2);

            entity.Property(x => x.Allowances)
                .HasPrecision(18, 2);

            entity.Property(x => x.GrossSalary)
                .HasPrecision(18, 2);

            entity.Property(x => x.Deductions)
                .HasPrecision(18, 2);

            entity.Property(x => x.NetSalary)
                .HasPrecision(18, 2);

            entity.Property(x => x.Status)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasOne(x => x.Employee)
                .WithMany(x => x.Payrolls)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x =>
                new
                {
                    x.EmployeeId,
                    x.PayrollDate
                });

            entity.HasIndex(x =>
                new
                {
                    x.Status,
                    x.PayrollDate
                });

            entity.HasOne(x => x.PayrollPeriod)
                .WithMany(x => x.Payrolls)
                .HasForeignKey(x => x.PayrollPeriodId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PayrollDeductionEntity>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Sss)
                .HasPrecision(18, 2);

            entity.Property(x => x.PhilHealth)
                .HasPrecision(18, 2);

            entity.Property(x => x.PagIbig)
                .HasPrecision(18, 2);

            entity.Property(x => x.WithholdingTax)
                .HasPrecision(18, 2);

            entity.Property(x => x.OtherDeductions)
                .HasPrecision(18, 2);

            entity.HasOne(x => x.Payroll)
                .WithOne(x => x.PayrollDeduction)
                .HasForeignKey<PayrollDeductionEntity>(
                    x => x.PayrollId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.PayrollId)
                .IsUnique();
        });

        modelBuilder.Entity<PayrollPeriodEntity>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PeriodCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.HasIndex(x => x.PeriodCode)
                .IsUnique();

            entity.HasIndex(x => new
            {
                x.Status,
                x.StartDate,
                x.EndDate
            });
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasOne(x => x.Employee).WithMany(x => x.Attendances)
                .HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.EmployeeId, x.AttendanceDate })
                .IsUnique().HasFilter("[IsDeleted] = 0");
        });
        modelBuilder.Entity<PayrollEntity>().Property(x => x.Status).IsConcurrencyToken();
        modelBuilder.Entity<PayrollPeriodEntity>().Property(x => x.Status).IsConcurrencyToken();
        modelBuilder.Entity<LeaveRequest>().Property(x => x.Status).IsConcurrencyToken();
        modelBuilder.Entity<LeaveBalance>().Property(x => x.UsedDays).IsConcurrencyToken();
        modelBuilder.Entity<LeaveBalance>().Property(x => x.AllocatedDays).IsConcurrencyToken();
        modelBuilder.Entity<LeaveBalance>().HasIndex(x => new { x.EmployeeId, x.Year, x.LeaveType })
            .IsUnique().HasFilter("[IsDeleted] = 0");
        modelBuilder.Entity<PayrollEntity>().HasIndex(x => new { x.EmployeeId, x.PayrollPeriodId })
            .IsUnique().HasFilter("[IsDeleted] = 0 AND [PayrollPeriodId] IS NOT NULL");

    }


    // =========================================================
    // SAVE CHANGES
    // =========================================================

    public override int SaveChanges()
    {
        ApplyAuditInformation();

        return base.SaveChanges();
    }


    // =========================================================
    // SAVE CHANGES ASYNC
    // =========================================================

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();

        return await base.SaveChangesAsync(cancellationToken);
    }


    // =========================================================
    // APPLY AUDIT INFORMATION
    // =========================================================

    private void ApplyAuditInformation()
    {
        var entries = ChangeTracker
            .Entries<AuditableEntity>()
            .Where(x =>
                x.State == EntityState.Added ||
                x.State == EntityState.Modified)
            .ToList();

        if (entries.Count == 0)
        {
            return;
        }


        var userName =
            _currentUserService.UserName ?? "system";

        var now =
            _dateTimeService.UtcNow;


        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                // =================================================
                // CREATE
                // =================================================

                case EntityState.Added:

                    entry.Entity.CreatedBy = userName;
                    entry.Entity.CreatedDate = now;

                    break;


                // =================================================
                // UPDATE
                // =================================================

                case EntityState.Modified:

                    entry.Property(x => x.CreatedBy).IsModified = false;
                    entry.Property(x => x.CreatedDate).IsModified = false;
                    if (IsSoftDelete(entry))
                    {
                        entry.Entity.DeletedBy = userName;
                        entry.Entity.DeletedDate = now;
                    }
                    if (!IsSoftDelete(entry))
                    {
                        entry.Entity.UpdatedBy = userName;
                        entry.Entity.UpdatedDate = now;
                    }

                    break;
            }
        }
    }


    // =========================================================
    // SOFT DELETE DETECTION
    // =========================================================

    private static bool IsSoftDelete(
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<AuditableEntity> entry)
    {
        var isDeletedProperty =
            entry.Metadata.FindProperty("IsDeleted");

        if (isDeletedProperty == null)
        {
            return false;
        }

        var originalValue =
            entry.Property("IsDeleted").OriginalValue;

        var currentValue =
            entry.Property("IsDeleted").CurrentValue;

        // false -> true = soft delete
        return originalValue is bool originalIsDeleted
            && currentValue is bool currentIsDeleted
            && !originalIsDeleted
            && currentIsDeleted;
    }
}