public class DynamicQuery
{
    public IEnumerable<Sort>? Sort { get; set; } // sıralama
    public Filter? Filter { get; set; } // filtreleme

    public DynamicQuery()
    {
        
    }

    public DynamicQuery(IEnumerable<Sort>? sort, Filter? filter)
    {
        Sort = sort;
        Filter = filter;
    }
}