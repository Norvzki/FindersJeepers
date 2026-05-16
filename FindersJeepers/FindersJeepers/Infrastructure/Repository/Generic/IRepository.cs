/// <summary>
/// A repository is an abstraction of a database relation. If you dont like to use the word relation, we can reword it:
/// A repository is an abstraction of a database table. In it, you have the basic operations we usually do to a table.
/// The basic inserting rows, deleting records, and of course, querying.
/// 
/// Anything you do here actually doesn't get reflected to the database yet until you tell UnitOfWork to do it via SaveChangesAsync()
/// </summary>
/// <typeparam name="T">Represents the type of entity used for the table. Just make sure this entity is registered as a DbSet in MyDbContext</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Insert a record to the database.
    /// </summary>
    /// <param name="entity">the record to be inserted</param>
    /// <returns></returns>
    Task AddAsync(T entity);
    /// <summary>
    /// Delete a record from the database. Usually involves by getting that entity from the database first via querying it.
    /// </summary>
    /// <param name="entity">The entity to be deleted.</param>
    void Delete(T entity);
    /// <summary>
    /// A query method, used to get a record via its primary key.
    /// </summary>
    /// <param name="id">The records' primary key</param>
    /// <returns></returns>
    Task<T> GetByIdAsync(int id);
    /// <summary>
    /// Update a record. Pass the new entity, the ORM will understand the changes that need to be made, 
    /// just as long as you didnt change the primary key, of course.
    /// </summary>
    /// <param name="entity">The updated entity.</param>
    void Update(T entity);
    /// <summary>
    /// Quite literally gets the entire table from the database. However, it doesn't mean that its loaded.
    /// It returns an IQueryable, which means that we still have to specify a query to actually fetch the records that we want.
    /// Querying is usually done via LINQ or foreach, whichever is more comfortable. You can use foreach if you don't know LINQ yet.
    /// </summary>
    /// <returns></returns>
    IQueryable<T> Get();
}
