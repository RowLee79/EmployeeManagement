using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

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
                EnsureSuccess(await roleManager.CreateAsync(new IdentityRole(role)));
            }
        }

        var configuration = services.GetRequiredService<IConfiguration>();
        var adminEmail = configuration["SeedAdmin:Email"];
        if (string.IsNullOrWhiteSpace(adminEmail)) return;
        var adminPassword = configuration["SeedAdmin:Password"];
        var admin =
            await userManager.FindByEmailAsync(
                adminEmail);

        if (admin is null)
        {
            if (string.IsNullOrWhiteSpace(adminPassword))
                throw new InvalidOperationException("Set SeedAdmin:Password using user secrets before creating the administrator.");
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
            EnsureSuccess(await userManager.AddToRoleAsync(admin, "Administrator"));
        }
    }

    private static void EnsureSuccess(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
    }
}
