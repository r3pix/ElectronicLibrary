using Microsoft.AspNetCore.Identity;

namespace ElectronicLibrary.Persistence.Seeder;

public static class RoleSeeder
{
    public const string AdminRole = "Admin";

    public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
    {
        if (!await roleManager.RoleExistsAsync(AdminRole))
            await roleManager.CreateAsync(new IdentityRole(AdminRole));
    }
}
