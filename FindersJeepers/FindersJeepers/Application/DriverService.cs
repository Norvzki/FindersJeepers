using Microsoft.EntityFrameworkCore;
using System.Data;
public class DriverService
{
    private readonly IUnitOfWork _uow;

    public DriverService(IUnitOfWork uow)
    {
        _uow = uow;
    }
    public async Task AddDriverAsync(string firstName, string lastName, string licenseNumber, string contactNumber, DateTime dateHired)
    {
        var transaction = _uow.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        try
        {
            var driver = Driver.Create(firstName, lastName, licenseNumber, contactNumber, dateHired);
            await _uow.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            await _uow.RollbackAsync();
            throw ex;
        }
        
    }
    public async Task AssignDriverToJeep(int driverId, int jeepId)
    {
        var driver = await _uow.Drivers.GetByIdAsync(driverId);
        var jeep = await _uow.Jeepneys.GetByIdAsync(jeepId);
        if (driver == null) throw new InvalidIdException("Invalid driver ID!");
        if (jeep == null) throw new InvalidIdException("Invalid jeep ID!");

        jeep.ChangeDriver(driver.Id); // wtf these methods are so THIN
    }

    public async Task<GetDriverResponse> GetDriverById(int driverId)
    {
        var driver = await _uow.Drivers.GetByIdAsync(driverId);
        if (driver == null) throw new InvalidIdException("Invalid driver ID!");

        return new GetDriverResponse
        {
            // stuff here
        };
     }
    public async Task<List<GetDriverResponse>> GetDrivers(int pageNumber, int pageSize)
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
    
    // DELETE DRIVERS HERE??



}



// gonna move these later to their own folders
public class InvalidIdException : Exception
{
    public InvalidIdException(string message) : base(message)
    {
    }
}

public record GetDriverResponse
{

}