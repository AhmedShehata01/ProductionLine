using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
            #region Seed Suber Admin User/Role/userClaim
            // Seed Roles
            var roles = new List<string> { "Super Admin", "Admin", "User" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var role = new ApplicationRole
                    {
                        Name = roleName,
                        NormalizedName = roleName.ToUpper(),
                        IsActive = true,
                        CreatedOn = DateTime.Now.ToString()
                    };
                    await roleManager.CreateAsync(role);
                }
            }

            var adminEmail = "SupAdmin@gmail.com";
            var adminPassword = "Abc@1234";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    NormalizedUserName = "Super Admin",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Super Admin");

                    // Adding claims to the Super Admin user
                    var claimsResult = await userManager.AddClaimAsync(adminUser, new Claim("View Role", "true"));
                    if (!claimsResult.Succeeded)
                    {
                        throw new InvalidOperationException($"Failed to add claim 'View Role': {string.Join(", ", claimsResult.Errors.Select(e => e.Description))}");
                    }

                    // Repeat for other claims
                    claimsResult = await userManager.AddClaimAsync(adminUser, new Claim("Create Role", "true"));
                    if (!claimsResult.Succeeded)
                    {
                        throw new InvalidOperationException($"Failed to add claim 'Create Role': {string.Join(", ", claimsResult.Errors.Select(e => e.Description))}");
                    }

                    claimsResult = await userManager.AddClaimAsync(adminUser, new Claim("Edit Role", "true"));
                    if (!claimsResult.Succeeded)
                    {
                        throw new InvalidOperationException($"Failed to add claim 'Edit Role': {string.Join(", ", claimsResult.Errors.Select(e => e.Description))}");
                    }

                    claimsResult = await userManager.AddClaimAsync(adminUser, new Claim("Delete Role", "true"));
                    if (!claimsResult.Succeeded)
                    {
                        throw new InvalidOperationException($"Failed to add claim 'Delete Role': {string.Join(", ", claimsResult.Errors.Select(e => e.Description))}");
                    }
                }

                else
                {
                    throw new InvalidOperationException("Failed to create admin user.");
                }
            }

            #endregion 

        }


    }
}
