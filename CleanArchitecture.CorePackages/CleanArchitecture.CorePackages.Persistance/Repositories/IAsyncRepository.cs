using System.Linq.Expressions;
using System.Reflection.Metadata;
using CleanArchitecture.CorePackages.Persistance.Repositories;
using Microsoft.EntityFrameworkCore.Query;

public interface IAsyncRepository<TEntity,TEntityId>:IQueryable<TEntity> where TEntity : Entity<TEntityId>
{
    Task<TEntity?> GetAsync
    (
        Expression<Func<TEntity, bool>> predicate, // where and linq expression
        Func<IQueryable<TEntity>,IIncludableQueryable<TEntity,object>>? include = null,  // join expression
        bool withDeleted = false, 
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );

    Task<Paginate<TEntity>> GetListAsync
    (
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );

    Task<Paginate<TEntity>> GetListByDynamicAsync
    (
        DynamicQuery dynamic,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>,IIncludableQueryable<TEntity,object>>? include = null,
        int index = 0,
        int size = 10,  
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );

    Task<bool> AnyAsync
    (
        Expression<Func<TEntity, bool>>? predicate=null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );

    Task<TEntity> AddAsync(TEntity entity);
    Task<ICollection<TEntity>> AddRangeAsync(ICollection<TEntity> entities);
    Task<TEntity> UpdateAsync(TEntity entity);
    Task<ICollection<TEntity>> UpdateRangeAsync(ICollection<TEntity> entities);
    Task<TEntity> DeleteAsync(TEntity entity,bool permanent = false);
    Task<ICollection<TEntity>> DeleteRangeAsync(ICollection<TEntity> entities,bool permanent = false);
}