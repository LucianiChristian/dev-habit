using DevHabit.Api.Database.Configurations;
using DevHabit.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Database;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    
    public DbSet<Tag> Tags { get; set; }
    
    public DbSet<Habit> Habits { get; set; }
    
    public DbSet<HabitTag> HabitTags { get; set; }
    
    public DbSet<GitHubAccessToken> GitHubAccessTokens { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Application);

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new TagConfiguration());
        modelBuilder.ApplyConfiguration(new HabitConfiguration());
        modelBuilder.ApplyConfiguration(new HabitTagConfiguration());
        modelBuilder.ApplyConfiguration(new GitHubAccessTokenConfiguration());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableDetailedErrors();
        optionsBuilder.EnableSensitiveDataLogging();
    }
}
