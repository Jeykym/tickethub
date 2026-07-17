using Microsoft.AspNetCore.Identity;

namespace tickethub.Seeders;

public class RoleSeeder
{
    public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles =
        [
            "Admin",
            "Customer",
            "Seller"
        ];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}