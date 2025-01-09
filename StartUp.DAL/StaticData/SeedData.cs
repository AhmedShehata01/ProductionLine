using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using StartUp.DAL.Extend;

namespace StartUp.DAL.StaticData
{
    public class SeedData
    {
        public static async Task SeedRolesAndAdminUser(IServiceProvider serviceProvider, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            // Seed Roles
            string[] roles = { "Super Admin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new ApplicationRole { Name = role });
                }
            }

            // Seed SuperAdmin User
            var adminEmail = "SupAdmin@gmail.com";
            var adminPassword = "Abc@1234"; // Ensure to use a valid hashed password or use `CreateAsync` to hash it.

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    NormalizedUserName = "Super Admin"
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Super Admin");
                }
            }
        }


    }
}
