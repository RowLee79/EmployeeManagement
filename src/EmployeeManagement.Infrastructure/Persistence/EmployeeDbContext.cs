using EmployeeManagement.Application.Common.Interfaces;
using EmployeeManagement.Domain.Common;
using EmployeeManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
    public DbSet<Payroll> Payrolls => Set<Payroll>();
    //public DbSet<PayrollItem> PayrollItems => Set<PayrollItem>();
    //public DbSet<PayrollItemType> PayrollItemTypes => Set<PayrollItemType>();
    //public DbSet<PayrollPeriod> PayrollPeriods => Set<PayrollPeriod>();

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
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Position)
                .WithMany()
                .HasForeignKey(x => x.PositionId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        // -----------------------------------------------------
        // Add your existing Department configuration here
        // -----------------------------------------------------
        //
        // Example:
        //
        // modelBuilder.Entity<Department>(entity =>
        // {
        //     entity.HasKey(x => x.Id);
        //
        //     entity.Property(x => x.Name)
        //         .HasMaxLength(200)
        //         .IsRequired();
        // });


        // -----------------------------------------------------
        // Add your existing Position configuration here
        // -----------------------------------------------------
        //
        // Example:
        //
        // modelBuilder.Entity<Position>(entity =>
        // {
        //     entity.HasKey(x => x.Id);
        //
        //     entity.Property(x => x.Code)
        //         .HasMaxLength(50)
        //         .IsRequired();
        //
        //     entity.Property(x => x.Name)
        //         .HasMaxLength(200)
        //         .IsRequired();
        // });
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