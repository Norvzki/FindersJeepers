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
        await _uow.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        try
        {
            var driver = Driver.Create(req.FirstName, req.LastName, req.LicenseNumber, req.ContactNumber, req.DateHired);
            await _uow.Drivers.AddAsync(driver);
            await _uow.SaveChangesAsync();
            await _uow.CommitAsync();
        }
        catch
        {
            await _uow.RollbackAsync();
            throw;
        }

    }
    public async Task<List<GetDriverResponse>> GetAsync(int pageNumber = -1, int pageSize = -1)
    {

        var query = _uow.Drivers.Get();

        if (pageNumber == -1 && pageSize == -1)
        {
            return await query.Select(d => new GetDriverResponse
            {
                FirstName = d.FirstName,
                DateHired = d.DateHired,
                ContactNumber = d.ContactNumber,
                Id = d.Id,
                LastName = d.LastName,
                LicenseNumber = d.LicenseNumber,
            }).ToListAsync();
        }

        var drivers = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return drivers.Select(d => new GetDriverResponse
        {
            FirstName = d.FirstName,
            DateHired = d.DateHired,
            ContactNumber = d.ContactNumber,
            Id = d.Id,
            LastName = d.LastName,
            LicenseNumber  = d.LicenseNumber,
        }).ToList();
    }

    // DELETE DRIVERS HERE?? SOFT DELETE OR HARD DELETE?
    // Sir ALLOWED Soft Deletes sooo lets softdelete driver here.

    public async Task DeleteAsync(int driverId)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateAsync(UpdateDriverRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<GetDriverDetailResponse> GetByIdAsync(int driverId)
    {
        var driver = await _uow.Drivers.GetByIdAsync(driverId);
        if (driver == null) throw new InvalidIdException("Invalid driver ID!");

        // this can actually be improved with single query.
        var jeepQuery = _uow.Jeepneys.Get();
        var routeQuery = _uow.Routes.Get();
        var tripQuery = _uow.Trips.Get();

        var jeepney = await jeepQuery.FirstOrDefaultAsync(x => x.DriverId == driver.Id);

        Route? route = null;                    // start as null first
        List<TripSummaryResponse>? trips = new();


        if (jeepney != null)
        {
            route = await routeQuery.FirstOrDefaultAsync(x => x.Id == jeepney.RouteId);
            trips = await tripQuery.Where(x => x.JeepneyId == jeepney.Id)
                    .Select(x => new TripSummaryResponse
                    {
                        Id = x.Id,
                        ArrivalTime = x.ArrivalTime,
                        DepartureTime = x.DepartureTime,
                        LogCount = x.Logs.Count,
                        RouteCode = route == null ? null : route.RouteCode,
                        Status = x.Status.ToString()
                    })
                    .ToListAsync();
        }

        return new GetDriverDetailResponse
        {
            Id = driver.Id,
            FirstName = driver.FirstName,
            LastName = driver.LastName,
            ContactNumber = driver.ContactNumber,
            LicenseNumber = driver.LicenseNumber,
            DateHired = driver.DateHired,

            TripHistory = trips,

            AssignedJeepney = jeepney == null ? null : new JeepneySummaryResponse
            {
                BodyNumber = jeepney.BodyNumber,
                Capacity = jeepney.Capacity,
                DriverName = driver.FirstName + " " + driver.LastName,
                PlateNumber = jeepney.PlateNumber,
                RouteCode = route.RouteCode
            },
        };

    }
}



// gonna move these later to their own folders
public class InvalidIdException : Exception
{
    public InvalidIdException(string message) : base(message)
    {
    }
}

