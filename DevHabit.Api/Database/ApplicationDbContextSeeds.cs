using System.Globalization;
using DevHabit.Api.Entities;

namespace DevHabit.Api.Database;

public static class ApplicationDbContextSeeds
{
    public static readonly IEnumerable<Habit> Habits =
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
