using DevHabit.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Database;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    
    public DbSet<Tag> Tags { get; set; }
    
    public DbSet<Habit> Habits { get; set; }
    
    public DbSet<HabitTag> HabitTags { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Application);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableDetailedErrors();
        optionsBuilder.EnableSensitiveDataLogging();
        
        // Synchronous seeding delegate
        optionsBuilder.UseSeeding((context, _) =>
        {
            if (context.Set<Habit>().Any())
            {
                return;
            }

            context.Set<Habit>().AddRange(ApplicationDbContextSeeds.Habits);
            context.SaveChanges();
        });

        // Asynchronous seeding delegate
        optionsBuilder.UseAsyncSeeding(async (context, _, cancellationToken) =>
        {
            if (await context.Set<Habit>().AnyAsync(cancellationToken))
            {
                return;
            }
            
            await context.Set<Habit>().AddRangeAsync(ApplicationDbContextSeeds.Habits, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        });
    }
}
