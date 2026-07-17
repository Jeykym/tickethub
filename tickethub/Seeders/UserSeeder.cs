using Microsoft.AspNetCore.Identity;

namespace tickethub.Seeders;

public class UserSeeder
{
    private static readonly (string Email, string Password, string Role)[] SeedUsers =
    [
        ("aliceadmin@tickethub.com", "Admin123!", "Admin"),
        ("bobcustomer@tickethub.com", "Customer123!", "Customer"),
        ("charlieseller@tickethub.com", "Seller123!", "Seller")
    ];

    public static async Task SeedAsync(UserManager<IdentityUser> userManager)
    {
        foreach (var (email, password, role) in SeedUsers)
        {
            await CreateUserAsync(userManager, email, password, role);
        }
    }

    private static async Task CreateUserAsync(
        UserManager<IdentityUser> userManager,
        string email,
        string password,
        string role)
    {
        var username = email.Split('@')[0];

        if (await userManager.FindByNameAsync(username) is not null)
            return;

        var user = new IdentityUser
        {
            UserName = username,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to seed user '{email}': {errors}");
        }
    }
}