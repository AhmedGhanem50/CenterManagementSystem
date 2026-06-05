using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CenterManagement.Domain.Entities;
using CenterManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CenterManagement.Infrastructure.Seed
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAndAdminAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            CenterManagementDbContext context)
        {
            // =========================
            // Roles
            // =========================

            string[] roles =
            {
                "Admin",
                "Instructor",
                "Student"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(role));
                }
            }

            // =========================
            // Admin User
            // =========================

            string adminEmail =
                "admin@center.com";

            var adminUser =
                await userManager.FindByEmailAsync(
                    adminEmail);

            if (adminUser == null)
            {
                var user = new ApplicationUser
                {
                    FullName = "System Admin",

                    UserName = adminEmail,

                    Email = adminEmail,

                    EmailConfirmed = true,

                    IsActive = true
                };

                var result =
                    await userManager.CreateAsync(
                        user,
                        "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(
                        user,
                        "Admin");
                }
            }

            // =========================
            // Seed GradeLevels
            // =========================

            if (!await context.GradeLevels.AnyAsync())
            {
                context.GradeLevels.AddRange(
                    new GradeLevel { Name = "Grade 1" },
                    new GradeLevel { Name = "Grade 2" },
                    new GradeLevel { Name = "Grade 3" },
                    new GradeLevel { Name = "Grade 4" },
                    new GradeLevel { Name = "Grade 5" }
                );
                await context.SaveChangesAsync();
            }

            // =========================
            // Seed INVALID Placeholder User
            // (for QrCodeLog FK when QR fails to decode)
            // =========================

            var invalidUser = await userManager.FindByIdAsync("INVALID");
            if (invalidUser == null)
            {
                var placeholder = new ApplicationUser
                {
                    Id = "INVALID",
                    FullName = "Invalid QR Placeholder",
                    UserName = "invalid@placeholder.local",
                    Email = "invalid@placeholder.local",
                    EmailConfirmed = true,
                    IsActive = false
                };
                await userManager.CreateAsync(placeholder, "Placeholder@999");
            }

            // =========================
            // Seed Subjects
            // =========================

            if (!await context.Subjects.AnyAsync())
            {
                context.Subjects.AddRange(
                    new Subject { Name = "Mathematics" },
                    new Subject { Name = "Science" },
                    new Subject { Name = "English" },
                    new Subject { Name = "History" },
                    new Subject { Name = "Art" }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}