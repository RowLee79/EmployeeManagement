using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagement.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        IServiceProvider services)
    {
        var roleManager =
            services.GetRequiredService<
                RoleManager<IdentityRole>>();

        var userManager =
            services.GetRequiredService<
                UserManager<ApplicationUser>>();

        string[] roles =
        {
            "Administrator",
            "HR Manager",
            "Manager",
            "Employee"
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(
                    new IdentityRole(role));
            }
        }

        // Create default administrator
        const string adminEmail =
            "admin@employeemanagement.local";

        const string adminPassword =
            "Admin@12345";

        var admin =
            await userManager.FindByEmailAsync(
                adminEmail);

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator"
            };

            var result =
                await userManager.CreateAsync(
                    admin,
                    adminPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    result.Errors.Select(x => x.Description));

                throw new InvalidOperationException(
                    $"Unable to create administrator: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(
                admin,
                "Administrator"))
        {
            await userManager.AddToRoleAsync(
                admin,
                "Administrator");
        }
    }
}