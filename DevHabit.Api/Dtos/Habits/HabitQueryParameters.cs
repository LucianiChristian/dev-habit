using DevHabit.Api.Dtos.Common;

namespace DevHabit.Api.Dtos.Habits;

public sealed record HabitQueryParameters  : AcceptHeaderDto
{
    public string Id { get; init; }
    
    public string? Fields { get; init; }
}
