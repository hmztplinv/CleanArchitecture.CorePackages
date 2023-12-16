public class Paginate<T>:IPaginate<T>
{
    public Paginate()
    {
        Items=Array.Empty<T>();
    }
    public int Size { get; set; }
    public int Index { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public IList<T> Items { get; set; }
    public bool HasPreviousPage => Index > 0;
    public bool HasNextPage => Index + 1 < TotalPages;
}