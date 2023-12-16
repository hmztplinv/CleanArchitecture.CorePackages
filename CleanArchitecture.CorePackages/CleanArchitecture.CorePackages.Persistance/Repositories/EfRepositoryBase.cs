using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using CleanArchitecture.CorePackages.Persistance.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

public class EfRepositoryBase<TContext, TEntity, TId> : IAsyncRepository<TEntity, TId>, IRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TContext : DbContext
{
    protected readonly TContext _dbContext;

    public EfRepositoryBase(TContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        entity.CreatedDate=DateTime.UtcNow;
        await _dbContext.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task<ICollection<TEntity>> AddRangeAsync(ICollection<TEntity> entities)
    {
        foreach(TEntity entity in entities)
            entity.CreatedDate=DateTime.UtcNow;
        await _dbContext.AddRangeAsync(entities);
        await _dbContext.SaveChangesAsync();
        return entities;
    }

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null, bool withDeleted = false, bool enableTracking = true, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> queryable=Query();
        if(!enableTracking)
            queryable=queryable.AsNoTracking();
        if(withDeleted)
            queryable=queryable.IgnoreQueryFilters();
        if(predicate!=null)
            queryable=queryable.Where(predicate);
        
        return await queryable.AnyAsync(cancellationToken);
    }

    public async Task<TEntity> DeleteAsync(TEntity entity, bool permanent = false)
    {
        await SetEntityAsDeletedAsync(entity,permanent);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task<ICollection<TEntity>> DeleteRangeAsync(ICollection<TEntity> entities, bool permanent = false)
    {
        await SetEntityAsDeletedAsync(entities,permanent);
        await _dbContext.SaveChangesAsync();
        return entities;
    }

    public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, bool withDeleted = false, bool enableTracking = true, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> queryable=Query();
        if(!enableTracking)
            queryable=queryable.AsNoTracking();
        if(include!=null)
            queryable=include(queryable);
        if(withDeleted)
            queryable=queryable.IgnoreQueryFilters();
        return await queryable.FirstOrDefaultAsync(predicate,cancellationToken);
    }

    public async Task<Paginate<TEntity>> GetListAsync(Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, int index = 0, int size = 10, bool withDeleted = false, bool enableTracking = true, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> queryable=Query();
        if(!enableTracking)
            queryable=queryable.AsNoTracking();
        if(include!=null)
            queryable=include(queryable);
        if(withDeleted)
            queryable=queryable.IgnoreQueryFilters();
        if(predicate!=null)
            queryable=queryable.Where(predicate);
        if(orderBy!=null)
            return await orderBy(queryable).PaginateAsync(index,size,cancellationToken);
        return await queryable.PaginateAsync(index,size,cancellationToken);
    }

    public async Task<Paginate<TEntity>> GetListByDynamicAsync(DynamicQuery dynamic, Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, int index = 0, int size = 10, bool withDeleted = false, bool enableTracking = true, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> queryable=Query().ToDynamic(dynamic);
        if(!enableTracking)
            queryable=queryable.AsNoTracking();
        if(include!=null)
            queryable=include(queryable);
        if(withDeleted)
            queryable=queryable.IgnoreQueryFilters();
        if(predicate!=null)
            queryable=queryable.Where(predicate);
        return await queryable.PaginateAsync(index,size,cancellationToken);
    }

    public IQueryable<TEntity> Query() => _dbContext.Set<TEntity>();
    

    public async Task<TEntity> UpdateAsync(TEntity entity)
    {
        entity.UpdatedDate=DateTime.UtcNow;
        _dbContext.Update(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task<ICollection<TEntity>> UpdateRangeAsync(ICollection<TEntity> entities)
    {
        foreach(TEntity entity in entities)
            entity.UpdatedDate=DateTime.UtcNow;
        _dbContext.UpdateRange(entities);
        await _dbContext.SaveChangesAsync();
        return entities;
    }

    protected async Task SetEntityAsDeletedAsync(TEntity entity,bool permanent)
    {
        if(!permanent)
        {
            CheckHasEntityHaveOneToOneRelation(entity);
            await SetEntityAsSoftDeletedAsync(entity);
        }
        else
        {
            _dbContext.Remove(entity);
        }
    }

    protected void CheckHasEntityHaveOneToOneRelation(TEntity entity)
    {
        bool hasEntityHaveOneToOneRelation=
            _dbContext
                    .Entry(entity)
                    .Metadata.GetForeignKeys()
                    .All(
                        x=> x.DependentToPrincipal?.IsCollection==true
                        || x.PrincipalToDependent?.IsCollection==true
                        || x.DependentToPrincipal?.ForeignKey.DeclaringEntityType.ClrType==entity.GetType()
                    )==false;
        if(hasEntityHaveOneToOneRelation)
            throw new InvalidOperationException("Entity has one to one relation.Soft delete causes data loss.");
    }

    protected async Task SetEntityAsSoftDeletedAsync(IEntityTimeStamps entity)
    {
        if(entity.DeletedDate.HasValue)
            return;
        entity.DeletedDate=DateTime.UtcNow;

        var navigations= _dbContext
            .Entry(entity)
            .Metadata.GetNavigations()
            .Where(x => x is { IsOnDependent:false,ForeignKey.DeleteBehavior:DeleteBehavior.ClientCascade or DeleteBehavior.Cascade})
            .ToList();
        
        foreach(INavigation? navigation in navigations)
        {
            if(navigation.TargetEntityType.IsOwned())
                continue;
            if(navigation.PropertyInfo is null)
                continue;
            
            object? navigationValue=navigation.PropertyInfo.GetValue(entity);
            if(navigation.IsCollection)
            {
                if(navigationValue == null)
                {
                    IQueryable queryable=_dbContext.Entry(entity).Collection(navigation.PropertyInfo.Name).Query();
                    navigationValue=await GetRelationLoaderQuery(queryable,navigationPropertyType:navigation.PropertyInfo.GetType()).ToListAsync();
                    if(navigationValue==null)
                        continue;
                }

                foreach(IEntityTimeStamps navValueItem in (IEnumerable)navigationValue)
                    await SetEntityAsSoftDeletedAsync(navValueItem);
            }
            else
            {
                if(navigationValue==null)
                {
                    IQueryable queryable=_dbContext.Entry(entity).Reference(navigation.PropertyInfo.Name).Query();
                    navigationValue=await GetRelationLoaderQuery(queryable,navigationPropertyType:navigation.PropertyInfo.GetType()).FirstOrDefaultAsync();
                    if(navigationValue==null)
                        continue;
                }
                await SetEntityAsSoftDeletedAsync((IEntityTimeStamps)navigationValue);
            }           
        }

        _dbContext.Update(entity);
    }

    protected IQueryable<object> GetRelationLoaderQuery(IQueryable queryable,Type navigationPropertyType)
    {
        Type queryProviderType=queryable.Provider.GetType();
        MethodInfo createQueryMethod=
            queryProviderType.GetMethods()
                .First(x=>x is { Name :nameof(queryable.Provider.CreateQuery), IsGenericMethod:true})
                ?.MakeGenericMethod(navigationPropertyType)
                ?? throw new InvalidOperationException("CreateQuery method not found.");
            var queryProviderQuery=(IQueryable<object>)createQueryMethod.Invoke(queryable.Provider,parameters:new object[]{queryable.Expression})!;
            return queryProviderQuery.Where(x => !((IEntityTimeStamps)x).DeletedDate.HasValue);
    }

    protected async Task SetEntityAsDeletedAsync(IEnumerable<TEntity> entities,bool permanent)
    {
        foreach(TEntity entity in entities)
            await SetEntityAsDeletedAsync(entity,permanent);
    }

    // Senkron Methods
    public TEntity? Get(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, bool withDeleted = false, bool enableTracking = true)
    {
        throw new NotImplementedException();
    }

    public IPaginate<TEntity> GetList(Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, int index = 0, int size = 10, bool withDeleted = false, bool enableTracking = true)
    {
        throw new NotImplementedException();
    }

    public IPaginate<TEntity> GetListByDynamic(DynamicQuery dynamic, Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, int index = 0, int size = 10, bool withDeleted = false, bool enableTracking = true)
    {
        throw new NotImplementedException();
    }

    public bool Any(Expression<Func<TEntity, bool>>? predicate = null, bool withDeleted = false, bool enableTracking = true)
    {
        throw new NotImplementedException();
    }

    public TEntity Add(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public ICollection<TEntity> AddRange(ICollection<TEntity> entities)
    {
        throw new NotImplementedException();
    }

    public TEntity Update(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public ICollection<TEntity> UpdateRange(ICollection<TEntity> entities)
    {
        throw new NotImplementedException();
    }

    public TEntity Delete(TEntity entity, bool permanent = false)
    {
        throw new NotImplementedException();
    }

    public ICollection<TEntity> DeleteRange(ICollection<TEntity> entities, bool permanent = false)
    {
        throw new NotImplementedException();
    }
}