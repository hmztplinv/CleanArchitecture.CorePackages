using Microsoft.EntityFrameworkCore;

public static class IQueryablePaginateExtensions
{
    public static async Task<Paginate<T>> PaginateAsync<T>
    (
        this IQueryable<T> query,
        int index,
        int size,
        CancellationToken cancellationToken = default
    )
    {
        int count = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        List<T> items = await query.Skip(index * size).Take(size).ToListAsync(cancellationToken).ConfigureAwait(false);
        Paginate<T> list=new()
        {
            Index = index,
            Size = size,
            TotalCount = count,
            TotalPages = (int)Math.Ceiling(count / (double)size),
            Items = items
        };
        return list;
    }
}