using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using tickethub.Models;

namespace tickethub;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Concert> Concerts { get; set; }
    public DbSet<Order> Orders { get; set; }
}