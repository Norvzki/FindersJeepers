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
        var plateNumberJeepney = await _uow.Jeepneys.Get()
    .Where(x => x.PlateNumber == request.PlateNumber)
    .FirstOrDefaultAsync();
        if (plateNumberJeepney != null)
            throw new ApplicationException("That plate number is already taken!");
        var bodyNumberJeepney = await _uow.Jeepneys.Get()
            .Where(x => x.BodyNumber == request.BodyNumber)
            .FirstOrDefaultAsync();
        if (bodyNumberJeepney != null)
            throw new ApplicationException("This body number is already taken!");

        var jeep = Jeepney.Create(request.PlateNumber, request.BodyNumber, request.Capacity, request.RouteId);
            await _uow.Jeepneys.AddAsync(jeep);
            await _uow.SaveChangesAsync();
    }
    public async Task<List<JeepneyDto>> GetAsync(int pageNumber = 1, int pageSize = 10)
    {
        return await (
        from j in _uow.Jeepneys.Get()
        join r in _uow.Routes.Get() on j.RouteId equals r.Id into routeGroup
        from subRoute in routeGroup.DefaultIfEmpty()
        select new JeepneyDto
        {
            Id = j.Id,
            PlateNumber = j.PlateNumber,
            BodyNumber = j.BodyNumber,
            Capacity = j.Capacity,
            RouteCode = subRoute != null ? subRoute.RouteCode : string.Empty,
            DriverCount = j.Drivers.Count(j0 => j0.UnassignedAt == null),
            TripCount = _uow.Trips.Get(null).Count(t => t.JeepneyId == j.Id)
        }
        )
        .ToListAsync();
    }
    public async Task<JeepneyDetail> GetDetail(int jeepId)
    {
        var jeep = await _uow.Jeepneys.GetByIdAsync(jeepId);
        if (jeep == null || jeep.IsDeleted) throw new InvalidIdException("Invalid jeepney ID!");

        string routecode = string.Empty;

        if (jeep.RouteId != null)
            routecode = (await _uow.Routes.GetByIdAsync((int)jeep.RouteId)).RouteCode;

        var drivers = await GetJeepneyDriversAsync(jeepId);

        var currentTrip = await (
            from t in _uow.Trips.Get()
            where t.JeepneyId == jeep.Id && (t.Status == TripStatus.OnGoing || t.Status == TripStatus.Waiting)
            join d in _uow.Drivers.Get(FetchOptions.IncludeDeleted) on t.DriverId equals d.Id
            join r in _uow.Routes.Get(FetchOptions.IncludeDeleted) on t.RouteId equals r.Id
            select new TripSummary
            {
                Id = t.Id,
                ArrivalTime = t.ArrivalTime,
                DepartureTime = t.DepartureTime,
                LogCount = t.Logs.Count,
                RouteCode = r.RouteCode,
                JeepneyPlateNumber = jeep.PlateNumber,
                Driver = new DriverSummary
                {
                    Id = d.Id,
                    Name = d.FirstName + " " + d.LastName,
                },
                Status = t.Status.ToString()
            }
        ).FirstOrDefaultAsync();

        var pastTrips = await (
            from t in _uow.Trips.Get()
            where t.JeepneyId == jeep.Id && t.Status == TripStatus.Completed
            join d in _uow.Drivers.Get(FetchOptions.IncludeDeleted) on t.DriverId equals d.Id
            join r in _uow.Routes.Get(FetchOptions.IncludeDeleted) on t.RouteId equals r.Id
            select new TripSummary
            {
                Id = t.Id,
                ArrivalTime = t.ArrivalTime,
                DepartureTime = t.DepartureTime,
                LogCount = t.Logs.Count,
                RouteCode = r.RouteCode,
                JeepneyPlateNumber = jeep.PlateNumber,
                Driver = new DriverSummary
                {
                    Id = d.Id,
                    Name = d.FirstName + " " + d.LastName,
                },
                Status = t.Status.ToString()
            }
        ).ToListAsync();

        return new JeepneyDetail
        {
            Id = jeep.Id,
            PlateNumber = jeep.PlateNumber,
            BodyNumber = jeep.BodyNumber,
            Capacity = jeep.Capacity,
            RouteCode = routecode,
            CurrentStatus = currentTrip != null ? "OnGoing" : "Waiting",
            CurrentTrip = currentTrip,
            AssignedDrivers = drivers,
            PastTrips = pastTrips,
        };
    }
    public async Task UpdateAsync(UpdateJeepneyRequest request)
    {
        var jeepney = await _uow.Jeepneys.GetByIdAsync(request.Id);

        var plateNumberJeepney = await _uow.Jeepneys.Get()
            .Where(x => x.PlateNumber == request.PlateNumber && x.Id != jeepney.Id)
            .FirstOrDefaultAsync();
        if (plateNumberJeepney != null)
            throw new ApplicationException("That plate number is already taken!");
        var bodyNumberJeepney = await _uow.Jeepneys.Get()
            .Where(x => x.BodyNumber == request.BodyNumber && x.Id != jeepney.Id)
            .FirstOrDefaultAsync();
        if (bodyNumberJeepney != null)
            throw new ApplicationException("This body number is already taken!");

        var currentTrip = await _uow.Trips.GetCurrentTripByJeepneyAsync(request.Id);
        if (currentTrip != null)
            throw new ApplicationException("A jeep cannot be updated if its currently on a trip!");


            jeepney.UpdateInformation(request.PlateNumber, request.BodyNumber, request.Capacity, request.RouteId);
            _uow.Jeepneys.Update(jeepney);
            await _uow.SaveChangesAsync();
    }
    public async Task DeleteAsync(int jeepId)
    {
        var jeepney = await _uow.Jeepneys.GetByIdAsync(jeepId);
        var currentTrip = await _uow.Trips.GetCurrentTripByJeepneyAsync(jeepId);
        if (currentTrip != null)
            throw new ApplicationException("A jeep cannot be deleted if its currently on a trip!");

        foreach(var d in jeepney.Drivers)
        {
            jeepney.RemoveDriver(d.DriverId);
        }

        jeepney.Delete();
        _uow.Jeepneys.Update(jeepney);
        await _uow.SaveChangesAsync();
    }
    public async Task AssignDriversAsync (AssignDriversRequest request)
    {
        var jeep = await _uow.Jeepneys.GetByIdAsync(request.JeepId);

        if (jeep == null)
            throw new Exception("Jeep not found!");

        //var driversToRemove = jeep.Drivers.Where(x => !request.DriverIds.Contains(x.DriverId)).ToList();

        //foreach (var remove in driversToRemove)
        //{
        //    jeep.RemoveDriver(remove.DriverId);
        //    _uow.Jeepneys.Update(jeep);
        //}

        foreach (var driver in request.DriverIds)
        {
            if (!jeep.IsADriver(driver))
            {
                jeep.AssignDriver(driver);
                _uow.Jeepneys.Update(jeep);
            }
        }
        await _uow.SaveChangesAsync();
    }

    public async Task<List<JeepneyDriverDto>> GetJeepneyDriversAsync(int jeepId)
    {
        var jeep = await _uow.Jeepneys.GetByIdAsync(jeepId);
        if (jeep == null) throw new InvalidIdException("Jeepney not found!");

        var activeJeepDrivers = jeep.Drivers
            .Where(j => j.UnassignedAt == null)
            .ToList();

        var activeDriverIds = activeJeepDrivers
            .Select(j => j.DriverId)
            .ToList();

        var drivers = await _uow.Drivers.Get()
            .Where(d => activeDriverIds.Contains(d.Id))
            .ToListAsync();

        var driverCurrentPlates = await (
            from t in _uow.Trips.Get()
            join j in _uow.Jeepneys.Get() on t.JeepneyId equals j.Id
            where activeDriverIds.Contains(t.DriverId) && t.ArrivalTime == null
            select new
            {
                t.DriverId,
                j.PlateNumber
            }
        ).ToListAsync();

        return (
            from d in drivers
            join jd in activeJeepDrivers on d.Id equals jd.DriverId
            let currentPlate = driverCurrentPlates.FirstOrDefault(p => p.DriverId == d.Id)?.PlateNumber
            select new JeepneyDriverDto
            {
                Id = d.Id,
                FirstName = d.FirstName,
                LastName = d.LastName,
                AssignedAt = jd.AssignedAt,
                IsAvailable = string.IsNullOrEmpty(currentPlate),
                CurrentJeepneyPlateDriving = currentPlate ?? "None"
            }
        ).ToList();
    }
    public async Task RemoveDriverAsync(int driverId, int jeepneyId)
    {
        var jeep = await _uow.Jeepneys.GetByIdAsync(jeepneyId);

        var currentTripByJeepney = await _uow.Trips.GetCurrentTripByJeepneyAsync(jeepneyId);

        if (currentTripByJeepney != null && currentTripByJeepney.DriverId == driverId)
            throw new ApplicationException("You cant really do that while he's currently driving the jeep now, can you?");

        jeep.RemoveDriver(driverId);
        _uow.Jeepneys.Update(jeep);
        await _uow.SaveChangesAsync();
    }
}

