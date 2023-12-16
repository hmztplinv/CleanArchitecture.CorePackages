public interface IPaginate<T>
{
    int Size { get; set; }
    int Index { get; set; }
    int TotalCount { get; set; }
    int TotalPages { get; set; }
    IList<T> Items { get; set; }
    bool HasPreviousPage { get; }
    bool HasNextPage { get; }
}