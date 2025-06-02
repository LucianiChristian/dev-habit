namespace DevHabit.Api.Dtos.Habits;

public sealed record UpsertHabitTagsDto
{
    public required List<string> TagIds { get; init; }
}
