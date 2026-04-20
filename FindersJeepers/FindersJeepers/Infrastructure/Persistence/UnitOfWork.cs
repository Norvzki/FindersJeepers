using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly MyDbContext _context;
    private IDbContextTransaction? _transaction;

    private IDriverRepository? _drivers;
    private IJeepneyRepository? _jeepneys;
    private ILocationRepository? _locations;
    private IRouteRepository? _routes;
    private ITripRepository? _trips;

    public UnitOfWork(MyDbContext context)
    {
        _context = context;
    }

    public IDriverRepository Drivers => _drivers ??= new DriverRepository(_context);
    public IJeepneyRepository Jeepneys => _jeepneys ??= new JeepneyRepository(_context);
    public ILocationRepository Locations => _locations ??= new LocationRepository(_context);
    public IRouteRepository Routes => _routes ??= new RouteRepository(_context);
    public ITripRepository Trips => _trips ??= new TripRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync(IsolationLevel isolationLevel)
    {
        if (_transaction is not null)
            throw new InvalidOperationException("A transaction is already in progress.");

        _transaction = await _context.Database.BeginTransactionAsync(isolationLevel);
    }

    public async Task CommitAsync()
    {
        if (_transaction is null)
            throw new InvalidOperationException("No active transaction to commit.");

        try
        {
            await _context.SaveChangesAsync();
            await _transaction.CommitAsync();
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction is null) return;

        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}