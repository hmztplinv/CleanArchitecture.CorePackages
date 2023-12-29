public interface IQuery<T>
{
    IQueryable<T> Query(); // sql sorgusu döndürür
}