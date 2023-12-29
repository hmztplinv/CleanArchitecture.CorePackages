public class Filter
{
    public string Field { get; set; } // filtre hangi alan üzerinde uygulanacak
    public string? Value { get; set; } // filtre değeri
    public string Operator { get; set; } // filtre operatörü (==, !=, >, <, >=, <=, contains, startswith, endswith)
    public string? Logic { get; set; } // filtre mantıksal operatörü (and, or)
    public IEnumerable<Filter>? Filters { get; set; } // filtrelerin içinde filtre olması durumunda kullanılır

    public Filter()
    {
        Field = string.Empty;
        Operator = string.Empty;
    }

    public Filter(string field, string @operator)
    {
        Field = field;
        Operator = @operator;
    }

}