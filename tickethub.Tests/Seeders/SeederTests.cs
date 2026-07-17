using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using tickethub.Seeders;

namespace tickethub.Tests.Seeders;

public class SeederTests
{
    private static readonly (string Username, string Email, string Role)[] ExpectedUsers =
    [
        ("aliceadmin", "aliceadmin@tickethub.com", "Admin"),
        ("bobcustomer", "bobcustomer@tickethub.com", "Customer"),
        ("charlieseller", "charlieseller@tickethub.com", "Seller")
    ];

    private static readonly string[] ExpectedRoles = ["Admin", "Customer", "Seller"];

    private static ServiceProvider BuildServices()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("SeederTestDb_" + Guid.NewGuid()));

        services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task RoleSeeder_CreatesAllExpectedRoles()
    {
        var provider = BuildServices();
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

        await RoleSeeder.SeedAsync(roleManager);

        foreach (var role in ExpectedRoles)
        {
            Assert.True(await roleManager.RoleExistsAsync(role));
        }

        Assert.Equal(ExpectedRoles.Length, roleManager.Roles.Count());
    }

    [Fact]
    public async Task UserSeeder_CreatesAllExpectedUsers()
    {
        var provider = BuildServices();
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = provider.GetRequiredService<UserManager<IdentityUser>>();

        await RoleSeeder.SeedAsync(roleManager); // required setup: AddToRoleAsync needs roles to exist
        await UserSeeder.SeedAsync(userManager);

        foreach (var (username, email, _) in ExpectedUsers)
        {
            var user = await userManager.FindByNameAsync(username);

            Assert.NotNull(user);
            Assert.Equal(email, user.Email);
            Assert.True(user.EmailConfirmed);
        }

        Assert.Equal(ExpectedUsers.Length, userManager.Users.Count());
    }

    [Fact]
    public async Task Seeding_AssignsEachUserToCorrectRole()
    {
        var provider = BuildServices();
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = provider.GetRequiredService<UserManager<IdentityUser>>();

        await RoleSeeder.SeedAsync(roleManager);
        await UserSeeder.SeedAsync(userManager);

        foreach (var (username, _, role) in ExpectedUsers)
        {
            var user = await userManager.FindByNameAsync(username);
            Assert.NotNull(user);
            Assert.True(await userManager.IsInRoleAsync(user!, role));

            // Also confirm no unexpected roles snuck in
            var assignedRoles = await userManager.GetRolesAsync(user!);
            Assert.Single(assignedRoles);
            Assert.Equal(role, assignedRoles[0]);
        }
    }

    [Fact]
    public async Task Seeding_IsIdempotent_WhenRunTwice()
    {
        var provider = BuildServices();
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = provider.GetRequiredService<UserManager<IdentityUser>>();

        await RoleSeeder.SeedAsync(roleManager);
        await UserSeeder.SeedAsync(userManager);

        await RoleSeeder.SeedAsync(roleManager);
        await UserSeeder.SeedAsync(userManager);

        Assert.Equal(ExpectedRoles.Length, roleManager.Roles.Count());
        Assert.Equal(ExpectedUsers.Length, userManager.Users.Count());
    }
}