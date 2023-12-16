using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks.Dataflow;
using System.Windows.Markup;

public static class IQueryableDynamicFilterExtensions
{
    private static readonly string[] _orders = { "asc", "desc" };
    private static readonly string[] _logics = { "and", "or" };

    private static readonly IDictionary<string,string> _operators=new Dictionary<string, string>
    {
        {"eq","=="},
        {"neq","!="},
        {"lt","<"},
        {"lte","<="},
        {"gt",">"},
        {"gte",">="},
        {"contains","Contains"},
        {"doesnotcontain","DoesNotContain"},
        {"startswith","StartsWith"},
        {"endswith","EndsWith"},
        {"isnull","=="},
        {"isnotnull","!="},
        {"isempty","=="},
        {"isnotempty","!="}
    };

    public static IQueryable<T> ToDynamic<T>(this IQueryable<T> query, DynamicQuery dynamicQuery)
    {
        if (dynamicQuery.Filter is not null)
            query = Filter(query, dynamicQuery.Filter);

        if (dynamicQuery.Sort is not null && dynamicQuery.Sort.Any())
            query = Sort(query, dynamicQuery.Sort);
        return query;
    }

    private static IQueryable<T> Filter<T>(IQueryable<T> queryable,Filter filter)
    {
        IList<Filter> filters=GetAllFilters(filter);
        string?[] values=filters.Select(x => x.Value).ToArray();
        string where =Transform(filter,filters);
        if(!string.IsNullOrEmpty(where) && values != null)
            queryable=queryable.Where(where,values);
        return queryable;
    }

    private static IQueryable<T> Sort<T>(IQueryable<T> queryable,IEnumerable<Sort> sorts)
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
        IList<Filter> filters=new List<Filter>();
        GetFilters(filter,filters);
        return filters;
    }

    private static void GetFilters(Filter filter, IList<Filter> filters)
    {
        filters.Add(filter);
        if (filter.Filters is not null && filter.Filters.Any())
            foreach (Filter f in filter.Filters)
                GetFilters(f, filters);
    }

    public static string Transform(Filter filter, IList<Filter> filters)
    {
        if (string.IsNullOrEmpty(filter.Field))
            throw new ArgumentException("Field is invalid");
        if (string.IsNullOrEmpty(filter.Operator) || !_operators.ContainsKey(filter.Operator.ToLower()))
            throw new ArgumentException("Operator is invalid");

        int index = filters.IndexOf(filter);
        string comparison = _operators[filter.Operator.ToLower()];
        StringBuilder where = new();
        if (!string.IsNullOrEmpty(filter.Value))
        {
            if (filter.Operator.ToLower() == "contains" || filter.Operator.ToLower() == "doesnotcontain")
                where.Append($"!np({filter.Field}).{comparison}(@{index.ToString()})");
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