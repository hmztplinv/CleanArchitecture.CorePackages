using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks.Dataflow;
using System.Windows.Markup;

public static class IQueryableDynamicFilterExtensions
{
    private static readonly string[] _orders = { "asc", "desc" }; // sıralama yönleri
    private static readonly string[] _logics = { "and", "or" }; // mantıksal operatörler
    
    private static readonly IDictionary<string,string> _operators=new Dictionary<string, string>
    {
        {"eq","=="}, // equal
        {"neq","!="}, // not equal
        {"lt","<"}, // less than
        {"lte","<="}, // less than or equal
        {"gt",">"}, // greater than
        {"gte",">="}, // greater than or equal
        {"contains","Contains"}, // contains
        {"doesnotcontain","DoesNotContain"}, // does not contain
        {"startswith","StartsWith"}, // starts with
        {"endswith","EndsWith"}, // ends with
        {"isnull","=="}, // is null
        {"isnotnull","!="}, // is not null
        {"isempty","=="}, // is empty
        {"isnotempty","!="} // is not empty
    };

    public static IQueryable<T> ToDynamic<T>(this IQueryable<T> query, DynamicQuery dynamicQuery) // IQueryable<T> yi DynamicQuery'e çevirir
    {
        if (dynamicQuery.Filter is not null)
            query = Filter(query, dynamicQuery.Filter);

        if (dynamicQuery.Sort is not null && dynamicQuery.Sort.Any())
            query = Sort(query, dynamicQuery.Sort);
        return query;
    }

    private static IQueryable<T> Filter<T>(IQueryable<T> queryable,Filter filter) // filtrelerin uygulanması
    {
        IList<Filter> filters=GetAllFilters(filter);
        string?[] values=filters.Select(x => x.Value).ToArray();
        string where =Transform(filter,filters);
        if(!string.IsNullOrEmpty(where) && values != null)
            queryable=queryable.Where(where,values);
        return queryable;
    }

    private static IQueryable<T> Sort<T>(IQueryable<T> queryable,IEnumerable<Sort> sorts) // sıralamanın uygulanması
    {
        foreach (Sort sort in sorts)
        {
            if(string.IsNullOrEmpty(sort.Field))
                throw new ArgumentException("Field is invalid");
            if(string.IsNullOrEmpty(sort.Dir) || !_orders.Contains(sort.Dir.ToLower()))
                throw new ArgumentException("Dir is invalid");
        }

        if(sorts.Any())
        {
            string orderBy =string.Join(separator: "," , values: sorts.Select(x => $"{x.Field} {x.Dir}"));
            queryable=queryable.OrderBy(orderBy);
        }
        return queryable;
    }

    public static IList<Filter> GetAllFilters(Filter filter)
    {
        IList<Filter> filters=new List<Filter>(); // filtrelerin hepsini alır
        GetFilters(filter,filters); 
        return filters;
    }

    private static void GetFilters(Filter filter, IList<Filter> filters) // filtreleri alır
    {
        filters.Add(filter); // filtrelerin varsa alt filtrelerini de alır
        if (filter.Filters is not null && filter.Filters.Any())
            foreach (Filter f in filter.Filters)
                GetFilters(f, filters);
    }

    public static string Transform(Filter filter, IList<Filter> filters) // filtreleri dönüştürür
    {
        if (string.IsNullOrEmpty(filter.Field))
            throw new ArgumentException("Field is invalid");
        if (string.IsNullOrEmpty(filter.Operator) || !_operators.ContainsKey(filter.Operator.ToLower()))
            throw new ArgumentException("Operator is invalid");

        int index = filters.IndexOf(filter); // filtrelerin indexini alır
        string comparison = _operators[filter.Operator.ToLower()]; // filtrelerin operatörlerini alır
        StringBuilder where = new(); // filtreleri oluşturur
        if (!string.IsNullOrEmpty(filter.Value))
        {
            if (filter.Operator.ToLower() == "contains" || filter.Operator.ToLower() == "doesnotcontain") // where ile StringBuilder
                where.Append($"!np({filter.Field}).{comparison}(@{index.ToString()})"); // np: null propagation, null verileri de ver ya da verme
            else if (comparison == "StartsWith" || comparison == "EndsWith" || comparison == "Contains")
                where.Append($"np({filter.Field}).{comparison}(@{index.ToString()})");
            else
                where.Append($"np({filter.Field}).{comparison}(@{index.ToString()})");
        }
        else if (filter.Operator is "isnull" || filter.Operator is "isnotnull")
            where.Append($"np({filter.Field}).{comparison}");
        else if (filter.Operator is "isempty" || filter.Operator is "isnotempty")
            where.Append($"np({filter.Field}).{comparison}");
        
        if (filter.Logic is not null && filter.Filters is not null && filter.Filters.Any())
        {
            if (!_logics.Contains(filter.Logic.ToLower()))
                throw new ArgumentException("Logic is invalid");
            return $"({where.ToString()} {filter.Logic.ToLower()} ({string.Join(separator: $" {filter.Logic.ToLower()} ", values: filter.Filters.Select(x => Transform(x, filters)))}))";
        }
        
        return where.ToString();
    }

}