using Microsoft.EntityFrameworkCore;
using tickethub.Models;

namespace tickethub;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {}
    
    public DbSet<Concert> Concerts { get; set; }
}