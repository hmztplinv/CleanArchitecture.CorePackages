public abstract class BasePageableModel // abstract yaptık çünkü bu sınıfı new'leyemeyeceğiz
{
    public int Size { get; set; }
    public int Index { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}