public class GetListResponse<T> : BasePageableModel // BasePageableModel index, size, totalCount, totalPages, hasPreviousPage, hasNextPage gibi özellikleri içeriyor
{
    private IList<T> _items; // T tipinde bir liste oluşturduk

    public IList<T> Items
    {
        get => _items ?? (_items = new List<T>()); // eğer _items null ise yeni bir liste oluşturup döndürüyoruz
        set => _items = value; // _items değerini set ediyoruz
    }
}