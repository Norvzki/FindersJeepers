using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Query.Internal;
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
        var driver = Driver.Create(req.FirstName, req.LastName, req.LicenseNumber, req.ContactNumber, req.DateHired);
        await _uow.Drivers.AddAsync(driver);
        await _uow.SaveChangesAsync();
    }
    public async Task<List<DriverDto>> GetAsync(int pageNumber = -1, int pageSize = -1)
    {

        var query = _uow.Drivers.Get();

        if (pageNumber == -1 && pageSize == -1)
        {
            return await query.Select(d => new DriverDto
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

        return drivers.Select(d => new DriverDto
        {
            FirstName = d.FirstName,
            DateHired = d.DateHired,
            ContactNumber = d.ContactNumber,
            Id = d.Id,
            LastName = d.LastName,
            LicenseNumber = d.LicenseNumber,
        }).ToList();
    }

    // DELETE DRIVERS HERE?? SOFT DELETE OR HARD DELETE?
    // Sir ALLOWED Soft Deletes sooo lets softdelete driver here.

    public async Task DeleteAsync(int driverId)
    {
        var driver = await _uow.Drivers.GetByIdAsync(driverId);
        var currentTrip = await _uow.Trips.GetCurrentTripByDriverAsync(driverId);
        if (currentTrip != null)
            throw new ApplicationException("A driver cannot be deleted if they're currently on a trip!");



        driver.Delete();
        _uow.Drivers.Update(driver);
        await _uow.SaveChangesAsync();
    }

    public async Task UpdateAsync(UpdateDriverRequest request)
    {
        var driver = await _uow.Drivers.GetByIdAsync(request.Id);
        var currentTrip = await _uow.Trips.GetCurrentTripByDriverAsync(request.Id);
        if (currentTrip != null)
            throw new ApplicationException("A driver cannot be updated if they're currently on a trip!");


        driver.UpdateInformation(request.FirstName, request.LastName, request.LicenseNumber, request.ContactNumber);

        _uow.Drivers.Update(driver);
        await _uow.SaveChangesAsync();
    }

    public async Task<DriverDetail> GetDetail(int driverId)
    {
        var driver = await _uow.Drivers.GetByIdAsync(driverId);
        if (driver == null) throw new InvalidIdException("Invalid driver ID!");

        var jeepneyData = await _uow.Jeepneys.Get(FetchOptions.IncludeDeleted)
            .Where(j => j.Drivers.Any(d => d.DriverId == driverId && d.UnassignedAt == null))
            .Join(_uow.Routes.Get(),
                j => j.RouteId,
                r => r.Id,
                (j, r) => new { Jeepney = j, Route = r })
            .ToListAsync();

        var assignedJeepneys = new List<JeepneySummary>();

        foreach (var x in jeepneyData)
        {
            var currentTrip = await _uow.Trips.GetCurrentTripByJeepneyAsync(x.Jeepney.Id);

            string driverName = string.Empty;
            if (currentTrip != null)
            {
                var d = await _uow.Drivers.GetByIdAsync(currentTrip.DriverId);
                if (d != null)
                {
                    driverName = $"{d.FirstName} {d.LastName}";
                }
            }

            assignedJeepneys.Add(new JeepneySummary
            {
                Id = x.Jeepney.Id,
                PlateNumber = x.Jeepney.PlateNumber,
                BodyNumber = x.Jeepney.BodyNumber,
                Capacity = x.Jeepney.Capacity,
                RouteCode = x.Route.RouteCode,
                IsAvailable = currentTrip == null, // wtf?
                CurrentDriverName = driverName
            });
        }   

        var jeepneyIds = jeepneyData.Select(x => x.Jeepney.Id).ToList();
        var routesByJeepneyId = jeepneyData.ToDictionary(x => x.Jeepney.Id, x => x.Route.RouteCode);

        var trips = await _uow.Trips.Get()
            .Where(t=>t.DriverId == driverId)
            .Join(_uow.Jeepneys.Get(FetchOptions.IncludeDeleted), t=>t.JeepneyId, j=>j.Id, (t, j) => new {Trip = t, Jeepney = j})
            .Select(t => new TripSummary
            {
                Id = t.Trip.Id,
                ArrivalTime = t.Trip.ArrivalTime,
                DepartureTime = t.Trip.DepartureTime,
                LogCount = t.Trip.Logs.Count,
                RouteCode = routesByJeepneyId.GetValueOrDefault(t.Jeepney.Id, string.Empty),
                Status = t.Trip.Status.ToString(),
                JeepneyPlateNumber = t.Jeepney.PlateNumber
            })
            .ToListAsync();

        return new DriverDetail
        {
            Id = driver.Id,
            FirstName = driver.FirstName,
            LastName = driver.LastName,
            ContactNumber = driver.ContactNumber,
            LicenseNumber = driver.LicenseNumber,
            DateHired = driver.DateHired,
            AssignedJeepneys = assignedJeepneys,
            TripHistory = trips
        };
    }

    public async Task AssignJeepneysAsync(AssignJeepneysRequest request)
    {
        var driver = await _uow.Drivers.GetByIdAsync(request.DriverId);
        if (driver == null) throw new KeyNotFoundException("Driver not found");

        var currentJeeps = await _uow.Jeepneys.Get()
            .Where(x => x.Drivers.Any(d => d.DriverId == driver.Id && d.UnassignedAt == null))
            .ToListAsync();

        var currentJeepIds = currentJeeps.Select(x => x.Id).ToList();

        foreach (var jeep in currentJeeps.Where(j => !request.JeepIds.Contains(j.Id)))
        {
            jeep.RemoveDriver(driver.Id);
            _uow.Jeepneys.Update(jeep);
        }
        var idsToAdd = request.JeepIds.Except(currentJeepIds);
        foreach (var jeepId in idsToAdd)
        {
            var jeep = await _uow.Jeepneys.GetByIdAsync(jeepId);
            if (jeep != null)
            {
                jeep.AssignDriver(driver.Id);
                _uow.Jeepneys.Update(jeep);
            }
        }
        await _uow.SaveChangesAsync();
    }



}


