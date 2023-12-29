using Microsoft.EntityFrameworkCore;

public static class IQueryablePaginateExtensions
{
    public static async Task<Paginate<T>> PaginateAsync<T>
    (
        this IQueryable<T> query, // sorgu
        int index,
        int size,
        CancellationToken cancellationToken = default
    )
    {
        int count = await query.CountAsync(cancellationToken).ConfigureAwait(false); // toplam kayıt sayısı ve ConfigureAwait(false) ile async işlemi thread pool dışında çalıştırıyoruz
        List<T> items = await query.Skip(index * size).Take(size).ToListAsync(cancellationToken).ConfigureAwait(false); // sayfalama işlemi 
        Paginate<T> list=new() // sayfalama işlemi sonucu oluşan listeyi döndürüyoruz
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