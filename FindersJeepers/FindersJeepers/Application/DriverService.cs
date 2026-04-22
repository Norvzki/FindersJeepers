using Microsoft.EntityFrameworkCore;
using System.Data;
public class DriverService : IDriverService
{
    private readonly IUnitOfWork _uow;

    public DriverService(IUnitOfWork uow)
    {
        _uow = uow;
    }
    public async Task CreateAsync(CreateDriverRequest req)
    {
        var transaction = _uow.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        try
        {
            var driver = Driver.Create(req.FirstName, req.LastName, req.LicenseNumber, req.ContactNumber, req.DateHired);
            await _uow.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            await _uow.RollbackAsync();
            throw ex;
        }

    }
    public async Task AssignJeep(int driverId, int jeepId)
    {
        var driver = await _uow.Drivers.GetByIdAsync(driverId);
        var jeep = await _uow.Jeepneys.GetByIdAsync(jeepId);
        if (driver == null) throw new InvalidIdException("Invalid driver ID!");
        if (jeep == null) throw new InvalidIdException("Invalid jeep ID!");

        jeep.ChangeDriver(driver.Id); // wtf these methods are so THIN
    }

    public async Task<GetDriverResponse> GetByIdAsync(int driverId)
    {
        var driver = await _uow.Drivers.GetByIdAsync(driverId);
        if (driver == null) throw new InvalidIdException("Invalid driver ID!");

        return new GetDriverResponse
        {
            // stuff here
        };
    }
    public async Task<List<GetDriverResponse>> GetAsync(int pageNumber, int pageSize)
    {
        var query = _uow.Drivers.Get();

        var drivers = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return drivers.Select(d => new GetDriverResponse
        {

        }).ToList();
    }

    // DELETE DRIVERS HERE?? SOFT DELETE OR HARD DELETE?
    // Sir ALLOWED Soft Deletes sooo lets softdelete driver here.

    public async Task DeleteAsync(int driverId)
    {

    }

    public async Task GetTrips(int driverId)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(UpdateDriverRequest request)
    {
        throw new NotImplementedException();
    }

    Task<GetDriverDetailResponse> IDriverService.GetByIdAsync(int driverId)
    {
        throw new NotImplementedException();
    }

    public Task<GetDriverDetailResponse> GetDetail(int driverId)
    {
        throw new NotImplementedException();
    }
}



// gonna move these later to their own folders
public class InvalidIdException : Exception
{
    public InvalidIdException(string message) : base(message)
    {
    }
}

