using System.Data;

public interface IUnitOfWork
{
    IDriverRepository Drivers { get; }
    IJeepneyRepository Jeepneys { get; }
    ILocationRepository Locations { get; }
    IRouteRepository Routes { get; }
    ITripRepository Trips { get; }

    Task BeginTransactionAsync(IsolationLevel isolationLevel);
    Task CommitAsync();
    void Dispose();
    Task RollbackAsync();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}