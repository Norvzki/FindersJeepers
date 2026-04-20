
using Microsoft.EntityFrameworkCore;

public abstract class Repository<T> : IRepository<T> where T : class
{
    protected DbSet<T> _set;
    public Repository(MyDbContext context)
    {
        _set = context.Set<T>();
    }
    public virtual async Task AddAsync(T entity) => await _set.AddAsync(entity);
    public virtual void Update(T entity) => _set.Update(entity);
    public virtual async Task<T> GetByIdAsync(int id) => await _set.FindAsync(id);
    public virtual IQueryable<T> Get() => _set.AsNoTracking();
    public virtual void Delete(T entity) => _set.Remove(entity);

}