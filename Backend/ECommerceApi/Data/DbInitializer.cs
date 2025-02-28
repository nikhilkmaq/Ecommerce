using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ECommerceApi.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerceApi.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Create roles
                string[] roles = { "Admin", "User" };
                foreach (var roleName in roles)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                        logger.LogInformation($"Created role: {roleName}");
                    }
                }

                // Create admin user with a strong password
                string adminEmail = "admin@example.com";
                string adminPassword = "Admin123!@#"; // More secure password

                // Check if admin exists
                var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

                if (existingAdmin == null)
                {
                    logger.LogInformation("Admin user not found. Creating new admin user.");

                    var adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FirstName = "Admin",
                        LastName = "User",
                        EmailConfirmed = true // Important for login
                    };

                    // Create admin with password
                    var result = await userManager.CreateAsync(adminUser, adminPassword);

                    if (result.Succeeded)
                    {
                        logger.LogInformation($"Created admin user: {adminEmail}");

                        // Add to Admin role
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        logger.LogInformation($"Added {adminEmail} to Admin role");
                    }
                    else
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        logger.LogError($"Failed to create admin user: {errors}");
                    }
                }
                else
                {
                    logger.LogInformation("Admin user already exists.");

                    // Reset admin password to ensure it works
                    var token = await userManager.GeneratePasswordResetTokenAsync(existingAdmin);
                    var resetResult = await userManager.ResetPasswordAsync(existingAdmin, token, adminPassword);

                    if (resetResult.Succeeded)
                    {
                        logger.LogInformation("Admin password has been reset successfully.");
                    }
                    else
                    {
                        var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
                        logger.LogError($"Failed to reset admin password: {errors}");
                    }

                    // Ensure admin is in Admin role
                    if (!await userManager.IsInRoleAsync(existingAdmin, "Admin"))
                    {
                        await userManager.AddToRoleAsync(existingAdmin, "Admin");
                        logger.LogInformation($"Added existing user {adminEmail} to Admin role");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the database with admin user");
            }
        }
    }
}
