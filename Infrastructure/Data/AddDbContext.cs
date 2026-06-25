using Microsoft.EntityFrameworkCore;
using practiceStudioAssetManager.Core.Entities;
using practiceStudioAssetManager.Core.Constants;

namespace practiceStudioAssetManager.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<Hardware> Equipments => Set<Hardware>();
    public DbSet<Software> Licenses => Set<Software>();
    public DbSet<Engineer> Engineers => Set<Engineer>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<StudioSession> StudioSessions => Set<StudioSession>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(AppConstants.DbConnectionString);
    }
}