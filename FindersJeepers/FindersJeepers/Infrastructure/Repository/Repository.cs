
using Microsoft.EntityFrameworkCore;

public class Repository<T> where T : class
{
    private MyDbContext _context;
    private DbSet<T> _set;
    public Repository(MyDbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }
    public void Add(T entity) => _set.Add(entity);
    public void Update(T entity) => _set.Update(entity);
    public async Task<T> GetByIdAsync(int id) => await _set.FindAsync(id);
    public void Delete(T entity) => _set.Remove(entity);
}