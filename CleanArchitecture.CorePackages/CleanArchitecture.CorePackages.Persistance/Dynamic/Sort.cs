public class Sort
{
    public string Field { get; set; } // sıralama hangi alan üzerinde uygulanacak
    public string Dir { get; set; } // sıralama yönü (asc, desc)

    public Sort()
    {
        Field = string.Empty;
        Dir = string.Empty;
    }

    public Sort(string field, string dir)
    {
        Field = field;
        Dir = dir;
    }
}