
public interface IRepository<T> where T : class
{
    Task AddAsync(T entity);
    void Delete(T entity);
    Task<T> GetByIdAsync(int id);
    void Update(T entity);
    IQueryable<T> Get();
}