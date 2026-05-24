
using Microsoft.EntityFrameworkCore;

public abstract class Repository<T> : IRepository<T> where T : class
{
    protected DbSet<T> _set;
    protected MyDbContext _context;
    public Repository(MyDbContext context)
    {
        _set = context.Set<T>();
        _context = context;
    }
    public virtual async Task AddAsync(T entity) => await _set.AddAsync(entity);
    public virtual void Update(T entity) => _set.Update(entity);
    public virtual async Task<T> GetByIdAsync(int id) => await _set.FindAsync(id);
    public virtual IQueryable<T> Get(FetchOptions? options = null) => _set.AsNoTracking();
    public virtual void Delete(T entity) => _set.Remove(entity);

}

public enum FetchOptions
{
    IncludeDeleted
}