using DevHabit.Api.Entities;

namespace DevHabit.Api.Dtos.Habits;

internal static class HabitMappings
{
    public static void UpdateFromDto(this Habit habit, UpdateHabitDto dto)
    {
        habit.Name = dto.Name;
        habit.Description = dto.Description;
        habit.Type = dto.Type;
        habit.EndDate = dto.EndDate;
        
        habit.Frequency = new Frequency
        {
            Type = dto.Frequency.Type,
            TimesPerPeriod = dto.Frequency.TimesPerPeriod
        };

        habit.Target = new Target
        {
            Value = dto.Target.Value, 
            Unit = dto.Target.Unit
        };

        if (dto.Milestone is not null)
        {
            habit.Milestone ??= new Milestone();
            habit.Milestone.Target = dto.Milestone.Target;
        }
        
        habit.UpdatedAtUtc = DateTime.UtcNow;
    }
    
    public static HabitDto ToDto(this Habit habit)
    {
        return new HabitDto
        {
            Id = habit.Id,
            Name = habit.Name,
            Description = habit.Description,
            Type = habit.Type,
            Frequency = new FrequencyDto
            {
                Type = habit.Frequency.Type,
                TimesPerPeriod = habit.Frequency.TimesPerPeriod 
            },
            Target = new TargetDto
            {
                Value = habit.Target.Value,
                Unit = habit.Target.Unit
            },
            Status = habit.Status,
            IsArchived = habit.IsArchived,
            EndDate = habit.EndDate,
            Milestone = habit.Milestone != null ? new MilestoneDto
            {
                Target = habit.Milestone.Target,
                Current = habit.Milestone.Current
            } : null,
            CreatedAtUtc = habit.CreatedAtUtc,
            UpdatedAtUtc = habit.UpdatedAtUtc,
            LastCompletedAtUtc = habit.LastCompletedAtUtc
        };
    }
    
    public static Habit ToEntity(this CreateHabitDto habitDto)
    {
        return new Habit
        {
            Id = $"h_{Guid.CreateVersion7()}",
            Name = habitDto.Name, 
            Description = habitDto.Description, 
            Type = habitDto.Type, 
            Frequency = new Frequency
            {
                Type = habitDto.Frequency.Type, 
                TimesPerPeriod = habitDto.Frequency.TimesPerPeriod 
            }, 
            Target = new Target
            {
                Value = habitDto.Target.Value, 
                Unit = habitDto.Target.Unit 
            },
            Status = HabitStatus.Ongoing,
            IsArchived = false, 
            EndDate = habitDto.EndDate, 
            Milestone = habitDto.Milestone == null ? null : new Milestone
            {
                Target = habitDto.Milestone.Target,
                Current = habitDto.Milestone.Current 
            },
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}
