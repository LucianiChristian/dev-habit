using DevHabit.Api.Dtos.Habits;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Dtos.Common;

public sealed record PaginationResult<T> : ICollectionResponse<T>, ILinksResponse
{
    public required List<T> Items { get; init; }
    
    public required int Page { get; init; }
    
    public required int PageSize { get; init; }
    
    public required int TotalCount { get; init; }
    
    public List<LinkDto> Links { get; set; }

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;

    public static async Task<PaginationResult<T>> CreateAsync(
        IQueryable<T> query,
        int page,
        int pageSize
    )
    {
        int totalCount = await query.CountAsync();
        
        List<T> items = await query
            .Skip(pageSize * (page - 1))
            .Take(pageSize)
            .ToListAsync();

        return new PaginationResult<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
