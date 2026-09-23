using EmployeeManagement.Application.Attendance.Interfaces;
using EmployeeManagement.Application.Common.Interfaces;
using EmployeeManagement.Application.Positions.Interfaces;
using EmployeeManagement.Application.Dashboard.Interfaces;
using EmployeeManagement.Application.Departments.Interfaces;
using EmployeeManagement.Application.Employees.Interfaces;
using EmployeeManagement.Application.LeaveManagement.Interfaces;
using EmployeeManagement.Application.Reporting;
using EmployeeManagement.Application.Users.Interfaces;
using EmployeeManagement.Infrastructure.Identity;
using EmployeeManagement.Infrastructure.Persistence;
using EmployeeManagement.Infrastructure.Services;
using EmployeeManagement.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// MVC
// ============================================================

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

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

// Reporting Service

builder.Services.AddHttpClient<IReportingService, ReportingService>(
    (serviceProvider, client) =>
    {
        var configuration =
            serviceProvider.GetRequiredService<IConfiguration>();

        var baseUrl =
            configuration["Reporting:BaseUrl"];

        client.BaseAddress =
            new Uri(baseUrl!);
    });
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

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddScoped<IDashboardService, DashboardService>();

// ============================================================
// File Storage
// ============================================================

builder.Services.AddScoped<
    IFileStorageService,
    LocalFileStorageService>();


// ============================================================
// Build Application
// ============================================================

var app = builder.Build();


// ============================================================
// Middleware
// ============================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

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