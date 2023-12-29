public class Paginate<T>:IPaginate<T>
{
    public Paginate()
    {
        Items=Array.Empty<T>(); // default olarak boş bir liste döndürür
    }
    public int Size { get; set; } // her sayfada kaç kayıt olacak
    public int Index { get; set; } // kaçıncı sayfadasın
    public int TotalCount { get; set; } // toplam kaç kayıt var
    public int TotalPages { get; set; } // toplam kaç sayfa var
    public IList<T> Items { get; set; } // kayıtlar
    public bool HasPreviousPage => Index > 0; // önceki sayfa var mı
    public bool HasNextPage => Index + 1 < TotalPages; // sonraki sayfa var mı
}