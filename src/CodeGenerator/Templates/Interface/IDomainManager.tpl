namespace ${Namespace}.Interface;

/// <summary>
/// 仓储数据管理接口
/// </summary>
public interface IDomainManager<TEntity, TUpdate>
       where TEntity : EntityBase
{
    DataStoreContext Stores { get; init; }
    QuerySet<TEntity> Query { get; init; }
    CommandSet<TEntity> Command { get; init; }

    /// <summary>
    /// 获取当前对象,通常是在修改前进行查询
    /// </summary>
    /// <param name="id"></param>
    /// <param name="navigations">include </param>
    /// <returns></returns>
    Task<TEntity?> GetCurrent(Guid id, string[]? navigations = null);
    Task<TEntity> AddAsync(TEntity entity);
    Task<TEntity> UpdateAsync(TEntity entity, TUpdate dto);
    Task<TEntity?> DeleteAsync(Guid id);

    /// <summary>
    /// 查询对象
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    /// <param name="whereExp"></param>
    /// <returns></returns>
    Task<TDto?> FindAsync<TDto>(Expression<Func<TEntity, bool>>? whereExp) where TDto : class;

    /// <summary>
    /// 分页查询
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <typeparam name="TFilter"></typeparam>
    /// <param name="filter"></param>
    /// <returns></returns>
    Task<PageList<TItem>> FilterAsync<TItem, TFilter>(TFilter filter) where TFilter : FilterBase;
}
