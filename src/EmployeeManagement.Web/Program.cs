using EmployeeManagement.Application.Attendance.Interfaces;
using EmployeeManagement.Application.Common.Interfaces;
using EmployeeManagement.Application.Dashboard.Interfaces;
using EmployeeManagement.Application.Departments.Interfaces;
using EmployeeManagement.Application.Employees.Interfaces;
using EmployeeManagement.Application.LeaveManagement.Interfaces;
using EmployeeManagement.Application.Payroll.Interfaces;
using EmployeeManagement.Application.Payroll.Models;
using EmployeeManagement.Application.Payroll.Services;
using EmployeeManagement.Application.Positions.Interfaces;
using EmployeeManagement.Application.Reporting;
using EmployeeManagement.Application.Users.Interfaces;
using EmployeeManagement.Infrastructure.Identity;
using EmployeeManagement.Infrastructure.Persistence;
using EmployeeManagement.Infrastructure.Services;
using EmployeeManagement.Web.Logging;
using EmployeeManagement.Web.Services;
using EmployeeManagement.Web.Services.Payroll;
using EmployeeManagement.Web.Services.Reporting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License =
    LicenseType.Community;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();
// ============================================================
// MVC
// ============================================================

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
    options.Filters.Add<EmployeeManagement.Web.Filters.ConcurrencyExceptionFilter>();
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
// ============================================================
// Database
// ============================================================

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<EmployeeDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
    options.UseSqlServer(connectionString));

// Use native PDF/XLSX reports unless the optional Crystal service is selected.
if (string.Equals(builder.Configuration["Reporting:Provider"], "Crystal", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddHttpClient<IReportingService, ReportingService>();
else
    builder.Services.AddScoped<IReportingService, NativeReportingService>();
// ============================================================
// ASP.NET Core Identity
// ============================================================

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Password requirements
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;

        // User requirements
        options.User.RequireUniqueEmail = true;

        // Lockout
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);

        // Sign-in
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser().Build();
    options.AddPolicy(
        "CanViewEmployees",
        policy =>
        {
            policy.RequireRole(
                "Administrator",
                "HR Manager",
                "Manager");
        });

    options.AddPolicy(
        "CanManageEmployees",
        policy =>
        {
            policy.RequireRole(
                "Administrator",
                "HR Manager");
        });

    options.AddPolicy(
        "CanManageDepartments",
        policy =>
        {
            policy.RequireRole(
                "Administrator",
                "HR Manager");
        });

    options.AddPolicy(
        "CanManagePositions",
        policy =>
        {
            policy.RequireRole(
                "Administrator",
                "HR Manager");
        });

    options.AddPolicy(
        "CanManageUsers",
        policy =>
        {
            policy.RequireRole(
                "Administrator");
        });

    options.AddPolicy(
        "CanViewReports",
        policy =>
        {
            policy.RequireRole(
                "Administrator",
                "HR Manager",
                "Manager");
        });

    options.AddPolicy(
        "CanPermanentlyDeleteEmployees",
        policy =>
        {
            policy.RequireRole(
                "Administrator");
        });
});

// ============================================================
// Authentication Cookie
// ============================================================

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";

    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});


// ============================================================
// Application Services
// ============================================================

builder.Services.AddScoped<IEmployeeService, EmployeeService>();

builder.Services.AddScoped<IDepartmentService, DepartmentService>();

builder.Services.AddScoped<IPositionService, PositionService>();

builder.Services.AddScoped<IAttendanceService, AttendanceService>();

builder.Services.AddScoped<ILeaveService, LeaveService>();
builder.Services.AddScoped<ILeaveAllocationService, LeaveAllocationService>();

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IDateTimeService, DateTimeService>();

builder.Services.AddScoped<IPayrollService, PayrollService>();

builder.Services.AddScoped<IPayslipPdfService, PayslipPdfService>();
builder.Services.AddScoped<IPayrollDeductionCalculator, PayrollDeductionCalculator>();
builder.Services.Configure<
    List<PayrollTaxBracket>>(
        builder.Configuration
            .GetSection("Payroll:TaxBrackets"));
builder.Services.AddScoped<IPayrollTaxCalculator, PayrollTaxCalculator>();
builder.Services.AddScoped<IPayrollPeriodService, PayrollPeriodService>();
builder.Services.AddScoped<IPayrollCalculationEngine,PayrollCalculationEngine>();
// ============================================================
// File Storage
// ============================================================

builder.Services.AddScoped<
    IFileStorageService,
    LocalFileStorageService>();

builder.Services.Configure<PayrollDeductionSettings>(
    builder.Configuration.GetSection(
        "Payroll:Deductions"));
// ============================================================
// Build Application
// ============================================================

var app = builder.Build();


app.UseMiddleware<CorrelationIdMiddleware>();

app.UseExceptionHandler();

// ============================================================
// Middleware
// ============================================================

if (!app.Environment.IsDevelopment())
{
    //  app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();


// Authentication MUST come before Authorization
app.UseAuthentication();

app.UseAuthorization();


// ============================================================
// Routing
// ============================================================

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();


// ============================================================
// Database Migration + Seeding
// ============================================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // --------------------------------------------------------
    // Employee Management Database
    // --------------------------------------------------------

    var employeeDbContext =
        services.GetRequiredService<EmployeeDbContext>();

    await employeeDbContext.Database.MigrateAsync();
    if (app.Configuration.GetValue<bool>("Database:SeedDemoData"))
        await DatabaseSeeder.SeedAsync(employeeDbContext);


    // --------------------------------------------------------
    // Identity Database
    // --------------------------------------------------------

    var identityDbContext =
        services.GetRequiredService<ApplicationIdentityDbContext>();

    await identityDbContext.Database.MigrateAsync();


    // --------------------------------------------------------
    // Identity Roles + Admin User
    // --------------------------------------------------------

    await IdentitySeeder.SeedAsync(services);
}


// ============================================================
// Run
// ============================================================

app.Run();