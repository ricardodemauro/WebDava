using Microsoft.EntityFrameworkCore;
using WebDava.Entities;

namespace WebDava.Repositories;

public class WebDavDbContext : DbContext
{
    public DbSet<ResourceInfoEntity> Resources { get; set; }

    public WebDavDbContext(DbContextOptions<WebDavDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ResourceInfoEntity>()
            .HasIndex(r => r.Path)
            .IsUnique();
    }
}