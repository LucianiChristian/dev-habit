using System.Globalization;
using DevHabit.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Database;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Habit> Habits { get; set; }
    
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

            context.Set<Habit>().AddRange(InitialHabits);
            context.SaveChanges();
        });

        // Asynchronous seeding delegate
        optionsBuilder.UseAsyncSeeding(async (context, _, cancellationToken) =>
        {
            if (await context.Set<Habit>().AnyAsync(cancellationToken))
            {
                return;
            }
            
            await context.Set<Habit>().AddRangeAsync(InitialHabits, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        });
    }
    
    private static readonly IEnumerable<Habit> InitialHabits =
    [
        new Habit
        {
            Id = "h_01HNK4V8J5T6MW8X9Y0Z1A2B3C",
            Name = "Daily Meditation",
            Description = "Morning mindfulness practice",
            Type = HabitType.Measurable,
            Frequency = new Frequency
            {
                Type = FrequencyType.Daily,
                TimesPerPeriod = 1
            },
            Target = new Target
            {
                Value = 15,
                Unit = "minutes"
            },
            Status = HabitStatus.Ongoing,
            CreatedAtUtc = DateTime.SpecifyKind(DateTime.Parse("2025-02-03T08:00:00Z", CultureInfo.InvariantCulture), DateTimeKind.Utc),
            IsArchived = false
        },
        new Habit
        {
            Id = "h_01HNK4V8P7Q8RX9Y0Z1A2B3C4D",
            Name = "Read Book",
            Description = "30 pages per day",
            Type = HabitType.Measurable,
            Frequency = new Frequency
            {
                Type = FrequencyType.Daily,
                TimesPerPeriod = 1
            },
            Target = new Target
            {
                Value = 30,
                Unit = "pages"
            },
            Status = HabitStatus.Ongoing,
            CreatedAtUtc = DateTime.SpecifyKind(DateTime.Parse("2025-02-03T09:00:00Z", CultureInfo.InvariantCulture), DateTimeKind.Utc),
            IsArchived = false
        },
        new Habit
        {
            Id = "h_01HNK4V91A2B3C4D5E6F7G8H9I",
            Name = "Journal",
            Description = "Daily reflection",
            Type = HabitType.Binary,
            Frequency = new Frequency
            {
                Type = FrequencyType.Daily,
                TimesPerPeriod = 1
            },
            Target = new Target
            {
                Value = 0,
                Unit = ""
            },
            Status = HabitStatus.Ongoing,
            CreatedAtUtc = DateTime.SpecifyKind(DateTime.Parse("2025-02-03T12:00:00Z", CultureInfo.InvariantCulture), DateTimeKind.Utc),
            IsArchived = false
        }
    ];
}
