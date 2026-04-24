using Microsoft.EntityFrameworkCore;
using System.Data;

public class JeepService : IJeepService
{

    private readonly IUnitOfWork _uow;

    public JeepService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task CreateAsync(CreateJeepneyRequest request)
    {
            var jeep = Jeepney.Create(request.PlateNumber, request.BodyNumber, request.Capacity, request.RouteId);
            await _uow.Jeepneys.AddAsync(jeep);
            await _uow.SaveChangesAsync();
    }

    public async Task<List<GetJeepneyResponse>> GetAsync(int pageNumber = 1, int pageSize = 10)
    {
        return await (
            from j in _uow.Jeepneys.Get()
            join r in _uow.Routes.Get() on j.RouteId equals r.Id
            select new GetJeepneyResponse
            {
                Id = j.Id,
                PlateNumber = j.PlateNumber,
                BodyNumber = j.BodyNumber,
                Capacity = j.Capacity,
                RouteCode = r.RouteCode,
                DriverCount = j.Drivers.Count(j0 => j0.UnassignedAt == null),
                TripCount = _uow.Trips.Get().Count(t => t.JeepneyId == j.Id)
            }
        )
        .ToListAsync();
    }

    public async Task<GetJeepneyDetailResponse> GetByIdAsync(int jeepId)
    {
        var jeep = await _uow.Jeepneys.GetByIdAsync(jeepId);
        if (jeep == null) throw new InvalidIdException("Invalid jeepney ID!");

        var route = await _uow.Routes.GetByIdAsync(jeep.RouteId);

        var assignedDriverIds = jeep.Drivers
            .Where(jd => jd.UnassignedAt == null)
            .Select(jd => jd.DriverId)
            .ToList();

        var drivers = await _uow.Drivers.Get()
            .Where(d => assignedDriverIds.Contains(d.Id))
            .Select(d => new DriverSummary
            {
                Id = d.Id,
                Name = d.FirstName + " " + d.LastName
            })
            .ToListAsync();

        var currentTrip = await _uow.Trips.Get()
            .Where(t => t.JeepneyId == jeep.Id && t.Status == TripStatus.OnGoing)
            .Select(t => new TripSummaryResponse
            {
                Id = t.Id,
                ArrivalTime = t.ArrivalTime,
                DepartureTime = t.DepartureTime,
                LogCount = t.Logs.Count,
                RouteCode = route.RouteCode,
                Status = t.Status.ToString()
            })
            .FirstOrDefaultAsync();

        var pastTrips = await _uow.Trips.Get()
            .Where(t => t.JeepneyId == jeep.Id && t.Status == TripStatus.Completed)
            .Select(t => new TripSummaryResponse
            {
                Id = t.Id,
                ArrivalTime = t.ArrivalTime,
                DepartureTime = t.DepartureTime,
                LogCount = t.Logs.Count,
                RouteCode = route.RouteCode,
                Status = t.Status.ToString()
            })
            .ToListAsync();

        return new GetJeepneyDetailResponse
        {
            Id = jeep.Id,
            PlateNumber = jeep.PlateNumber,
            BodyNumber = jeep.BodyNumber,
            Capacity = jeep.Capacity,
            RouteCode = route.RouteCode,
            CurrentStatus = currentTrip != null ? "OnGoing" : "Waiting",
            CurrentTrip = currentTrip,
            AssignedDrivers = drivers,
            PastTrips = pastTrips
        };
    }

    public Task UpdateAsync(UpdateJeepneyRequest request)
    {
        throw new NotImplementedException();
    }


    public Task DeleteAsync(int jeepId)
    {
        throw new NotImplementedException();
    }


    public async Task AssignDriversAsync (AssignDriversRequest request)
    {
        var jeep = await _uow.Jeepneys.GetByIdAsync(request.JeepId);

        if (jeep == null)
            throw new Exception("Jeep not found!");

        var driversToRemove = jeep.Drivers.Where(x => !request.DriverIds.Contains(x.DriverId)).ToList();

        foreach (var remove in driversToRemove)
        {
            jeep.RemoveDriver(remove.DriverId);
        }

        foreach (var driver in request.DriverIds)
        {
            if(!jeep.IsADriver(driver))
                jeep.AssignDriver(driver);
        }
        await _uow.SaveChangesAsync();
    }
}

