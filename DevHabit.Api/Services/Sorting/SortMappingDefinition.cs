namespace DevHabit.Api.Services.Sorting;

#pragma warning disable S2326

public sealed class SortMappingDefinition<TSource, TDestination> : ISortMappingDefinition
{
    public required SortMapping[] Mappings { get; init; }
}
