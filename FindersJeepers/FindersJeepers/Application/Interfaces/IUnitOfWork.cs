using System.Data;

/// <summary>
/// Think of this as our "Database operations" as an object. In this object, we group all the repositories in one place.
/// When inserting rows, querying tables, deleting, etc, you do it through here.
/// 
/// This object is supposed to enforce ACID principles by grouping all of the repositories into one object along with the transaction
/// methods, but it just so happens that our use cases dont really involve that many entities at once anyways.
/// When accessing a repository, its best we do it here. Inject this object into your class if you want to use the services here.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Represents the Drivers table in the database.
    /// </summary>
    IDriverRepository Drivers { get; }
    /// <summary>
    /// Represents the Jeepneys table in the database.
    /// </summary>
    IJeepneyRepository Jeepneys { get; }
    /// <summary>
    /// Represents the Locations table in the database.
    /// </summary>
    ILocationRepository Locations { get; }
    /// <summary>
    /// Represents the Routes table in the database.
    /// </summary>
    IRouteRepository Routes { get; }
    /// <summary>
    /// Represents the Trips table in the database.
    /// </summary>
    ITripRepository Trips { get; }

    /// <summary>
    /// If your business logic involves ADDING/UPDATING ROWS of MULTIPLE TABLES, its important to put them into one transaction.
    /// This is because if one of those operations fail, we could have missing data.
    /// For example, when transferring funds from account A to account B, we need to make sure both operations succeed, otherwise we'll have problems
    /// Transactions make sure that all operations are "all-or-nothing". Either everything goes through, or we cancel it.
    /// </summary>
    /// <param name="isolationLevel">Defines the isolation level of the transaction.</param>
    /// <returns></returns>
    Task BeginTransactionAsync(IsolationLevel isolationLevel);
    /// <summary>
    /// Finalizes the changes made in the transaction you have made. Usually followed by a SaveChangesAsync()
    /// </summary>
    /// <returns></returns>
    Task CommitAsync();
    /// <summary>
    /// We usually dont use this. I think.
    /// </summary>
    void Dispose();
    /// <summary>
    /// If shit goes wrong, somehow, in your transaction, make sure you call this method so we can undo and cancel the pending changes to the
    /// database.
    /// </summary>
    /// <returns></returns>
    Task RollbackAsync();
    /// <summary>
    /// This persists the changes to the database. It does the final writing/updating. 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}