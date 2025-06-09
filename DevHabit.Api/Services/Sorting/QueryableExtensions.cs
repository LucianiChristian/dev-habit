using System.Linq.Dynamic.Core;

namespace DevHabit.Api.Services.Sorting;

internal static class QueryableExtensions
{
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> queryable,
        string? sort,
        SortMapping[] mappings,
        string defaultOrderBy = "Id")
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return queryable.OrderBy(defaultOrderBy);
        }

        string[] sortFields = sort.Split(",")
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();
        
        var orderByParts = new List<string>();

        foreach (string field in sortFields)
        {
            (string sortField, bool isDescending) = ParseSortField(field);

            SortMapping mapping =
                mappings.First(m => m.SortField.Equals(sortField, StringComparison.OrdinalIgnoreCase));

            string direction = (isDescending, mapping.Reverse) switch
            {
                (false, false) => "ASC",
                (false, true) => "DESC",
                (true, true) => "ASC",
                (true, false) => "DESC"
            };
            
            orderByParts.Add($"{mapping.PropertyName} {direction}");
        }

        string orderBy = string.Join(",", orderByParts);

        return queryable.OrderBy(orderBy);
    }

    private static (string SortField, bool IsDescending) ParseSortField(string field)
    {
        string[] parts = field.Split(' ');
        
        string sortField = parts[0];
        bool isDescending = parts.Length > 1 &&
                            parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

        return (sortField, isDescending);
    }
}
