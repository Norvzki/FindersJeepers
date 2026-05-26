using Microsoft.EntityFrameworkCore;
using MudBlazor;

public class TripService : ITripService
{
    private readonly IUnitOfWork _uow;

    public TripService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task CreateDriverTrip(StartTripRequest req)
    {
        var currentTripOfDriver = await _uow.Trips.GetCurrentTripByDriverAsync(req.DriverId);
        if (currentTripOfDriver != null)
            throw new ApplicationException("This driver is busy on a trip different!");

        var currentTripOfJeepney = await _uow.Trips.GetCurrentTripByJeepneyAsync((int)req.JeepId);
        if (currentTripOfJeepney != null)
            throw new ApplicationException("This jeepney is on a trip!");

        if (req.Direction == null)
        {
            var latestTrip = await _uow.Trips.Get()
                .OrderByDescending(t => t.ArrivalTime) 
                .FirstOrDefaultAsync();

            req.Direction = (latestTrip == null || latestTrip.Direction == RouteDirection.Return)
                ? RouteDirection.Forward
                : RouteDirection.Return;
        }

        int finalJeepId;
        if (req.JeepId != null)
        {
            finalJeepId = req.JeepId.Value;
            var isJeepneyOnATrip = await _uow.Trips.Get()
                .AnyAsync(x => x.JeepneyId == finalJeepId && x.Status == TripStatus.OnGoing);

            if (isJeepneyOnATrip)
                throw new ApplicationException("This jeepney is currently being driven by somebody else!");
        }
        else
        {
            var jeeps = await _uow.Jeepneys.GetByDriverAsync(req.DriverId);

            var selectedJeep = jeeps
                .Select(j => new {
                    Id = j.Id,
                    TripCount = _uow.Trips.Get().Count(t => t.JeepneyId == j.Id && t.Status == TripStatus.Completed)
                })
                .OrderBy(js => js.TripCount)
                .FirstOrDefault();

            if (selectedJeep == null)
                throw new ApplicationException("No available jeepneys found for this driver.");

            finalJeepId = selectedJeep.Id;
        }

        var jeepEntity = await _uow.Jeepneys.GetByIdAsync(finalJeepId);

        if (jeepEntity.RouteId == null)
            throw new ApplicationException("You cannot start a trip if a jeepney doesn't have a route!");

        var trip = Trip.Create(req.DriverId, jeepEntity.Id, (int)jeepEntity.RouteId, req.Direction.Value);

        await _uow.Trips.AddAsync(trip);
        await _uow.SaveChangesAsync();
    }

    // God forbid this was a BITCH to code not even AI was able to help me.
    // Sponsored by finite state machines.
    public async Task NextStop(NextStopRequest req)
    {
        var trip = await _uow.Trips.GetByIdAsync(req.TripId);
        if (trip == null || trip.IsDeleted) throw new InvalidIdException("Trip not found or that trip is deleted!");

        if (trip.Status != TripStatus.OnGoing) throw new DomainException("Trip is not ongoing!");

        var route = await _uow.Routes.GetByIdAsync(trip.RouteId);
        var jeepney = await _uow.Jeepneys.GetByIdAsync(trip.JeepneyId);

        bool isReturn = trip.Direction == RouteDirection.Return;

        // Resolve terminals based on direction
        int terminalStartId = isReturn ? route.LocationEndId : route.LocationStartId;
        int terminalEndId = isReturn ? route.LocationStartId : route.LocationEndId;

        // Use the correct directed stops (already ordered by StopIndex, reversed for Return via GenerateReturnStopsFromForward)
        var directedStops = isReturn
            ? route.ReturnStops.ToList()
            : route.ForwardStops.ToList();

        var lastLog = trip.Logs
            .OrderByDescending(x => x.TimeStamp)
            .FirstOrDefault();

        int stopId;
        TripLogType nextLogType;
        bool isTerminal = false;

        if (lastLog == null)
        {
            stopId = terminalStartId;
            nextLogType = TripLogType.Departure;
        }
        else if (lastLog.EventType == TripLogType.Arrival)
        {
            stopId = lastLog.LocationId;
            nextLogType = TripLogType.Departure;
        }
        else
        {
            if (lastLog.LocationId == terminalStartId)
            {
                var firstStop = directedStops.FirstOrDefault();

                if (firstStop == null)
                {
                    stopId = terminalEndId;
                    nextLogType = TripLogType.Arrival;
                    isTerminal = true;
                }
                else
                {
                    stopId = firstStop.LocationId;
                    nextLogType = TripLogType.Arrival;
                }
            }
            else
            {
                var currentStop = directedStops.FirstOrDefault(s => s.LocationId == lastLog.LocationId);
                var nextStop = directedStops.FirstOrDefault(s => s.StopIndex == currentStop.StopIndex + 1);

                if (nextStop == null)
                {
                    stopId = terminalEndId;
                    nextLogType = TripLogType.Arrival;
                    isTerminal = true;
                }
                else
                {
                    stopId = nextStop.LocationId;
                    nextLogType = TripLogType.Arrival;
                }
            }
        }

        trip.LogStopEvent(stopId, req.PassengerCount, jeepney.Capacity, nextLogType);

        if (isTerminal && nextLogType == TripLogType.Arrival)
            trip.CompleteTrip();

        _uow.Trips.Update(trip);
        await _uow.SaveChangesAsync();
    }

    public async Task<List<TripDto>> GetTripsAsync()
    {
        return await (
            from t in _uow.Trips.Get()
            join j in _uow.Jeepneys.Get() on t.JeepneyId equals j.Id
            join r in _uow.Routes.Get() on t.RouteId equals r.Id
            select new TripDto
            {
                PlateNumber = j.PlateNumber,
                ArrivalTime = t.ArrivalTime,
                DepartureTime = t.DepartureTime,
                Id = t.Id,
                LogCount = t.Logs.Count,
                RouteCode = $"{r.RouteCode} ({t.Direction.ToString()})",
                Status = t.Status.ToString()
            }
            ).ToListAsync();
    }

    public async Task<TripDetailDto> GetDetailAsync(int tripId)
    {
        var trip = await _uow.Trips.GetByIdAsync(tripId);

        if (trip == null || trip.IsDeleted) throw new InvalidIdException("That trip is already deleted!");


        return await (
            from t in _uow.Trips.Get(FetchOptions.IncludeDeleted)
            where t.Id == tripId
            join j in _uow.Jeepneys.Get(FetchOptions.IncludeDeleted) on t.JeepneyId equals j.Id
            join r in _uow.Routes.Get(FetchOptions.IncludeDeleted) on t.RouteId equals r.Id
            select new TripDetailDto
            {
                ArrivalTime = t.ArrivalTime,
                Capacity = j.Capacity,
                DepartureTime = t.DepartureTime,
                Id = t.Id,
                JeepneyId = j.Id,
                PlateNumber = j.PlateNumber,
                RouteCode = $"{r.RouteCode} ({t.Direction.ToString()})",
                RouteId = r.Id,
                Status = t.Status.ToString(),
                Logs = (
                from tl in t.Logs
                join l in _uow.Locations.Get(FetchOptions.IncludeDeleted) on tl.LocationId equals l.Id
                select new TripLogDto
                {
                    EventType = tl.EventType.ToString(),
                    PassengerCount = tl.PassengerCount,
                    StopName = l.Name,
                    Timestamp = tl.TimeStamp,
                }
                ).ToList()
            }
            ).FirstOrDefaultAsync();
    }
    public async Task StartTrip(int tripId)
    {
        var trip = await _uow.Trips.GetByIdAsync(tripId);

        if (trip == null)
            throw new KeyNotFoundException("Trip not found!");

        trip.StartTrip();
        _uow.Trips.Update(trip);
        await _uow.SaveChangesAsync();
    }
    public async Task CompleteTrip(int tripId)
    {
        var trip = await _uow.Trips.GetByIdAsync(tripId);

        if (trip == null)
            throw new KeyNotFoundException("Trip not found!");

        trip.CompleteTrip();
        _uow.Trips.Update(trip);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(int tripId)
    {
        var trip = await _uow.Trips.GetByIdAsync(tripId);
        if (trip == null || trip.IsDeleted) throw new InvalidIdException("This trip is deleted or id is invalid");

        if (trip.Status != TripStatus.Completed)
            throw new ApplicationException("You cannot delete an active trip!");

        trip.Delete();
        _uow.Trips.Update(trip);
        await _uow.SaveChangesAsync();
    }
}
