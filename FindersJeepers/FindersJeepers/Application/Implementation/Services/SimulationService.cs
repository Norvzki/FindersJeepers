using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Simulates a full 24-hour operational day for the jeepney network.
///
/// TIME SCALE: 1 real second = 1 simulated hour.
///             Implemented as a timer that fires every 1ms.
///             1000 ticks = 1 simulated hour. 24000 ticks = full day.
///
/// Each tick the service decides what to do based on the simulated hour:
///   - Rush hours  → spawn new trips, aggressively drive ongoing trips forward
///   - Midday/lull → admin ops (updates, reassignments, reads)
///   - Always      → drive ALL currently ongoing trips one step forward per tick
///
/// HOW NextStop WORKS (FSM, must call in sequence per trip):
///   No logs           → Departure from terminal start
///   After Departure   → Arrival at next stop (or terminal end → auto-completes)
///   After Arrival     → Departure from that same stop
///   Repeat until terminal end Arrival → trip auto-completes inside TripService.
///
/// So to fully run a trip we just keep calling NextStop until the trip disappears
/// from the OnGoing list (it will be Completed).
/// </summary>
public class SimulationService : ISimulationService
{
    private readonly IDriverService _driverService;
    private readonly IJeepService _jeepService;
    private readonly ILocationService _locationService;
    private readonly IRouteService _routeService;
    private readonly ITripService _tripService;

    private static readonly Random _rng = new();

    // ── Simulated clock state ──────────────────────────────────────────────
    private int _simHour = 0;   // 0–23
    private int _tickCount = 0;   // increments every ms; every 1000 ticks = 1 sim hour

    // Ticks per simulated hour (1 real second = 1 sim hour → 1000 ms = 1000 ticks)
    private const int TicksPerHour = 1000;
    private const int TotalHours = 24;
    private const int TotalTicks = TicksPerHour * TotalHours; // 24 000

    // ── Passenger load by hour (index = simulated hour 0–23) ──────────────
    // Reflects real Cebu jeepney ridership patterns
    private static readonly (int Min, int Max)[] _paxByHour = new (int, int)[24]
    {
        (0,  2),  //  0 AM — dead of night
        (0,  2),  //  1 AM
        (0,  1),  //  2 AM
        (0,  1),  //  3 AM
        (1,  5),  //  4 AM — first departures
        (3,  10), //  5 AM — early commuters
        (8,  18), //  6 AM — morning rush building
        (12, 22), //  7 AM — PEAK morning rush
        (12, 22), //  8 AM — PEAK morning rush
        (6,  16), //  9 AM — taper
        (4,  12), // 10 AM
        (5,  14), // 11 AM — lunch build-up
        (7,  18), // 12 PM — lunch rush
        (5,  14), //  1 PM
        (3,  10), //  2 PM — afternoon lull
        (3,  10), //  3 PM
        (6,  16), //  4 PM — evening rush building
        (12, 22), //  5 PM — PEAK evening rush
        (12, 22), //  6 PM — PEAK evening rush
        (7,  16), //  7 PM — taper
        (4,  12), //  8 PM
        (2,  8),  //  9 PM
        (1,  5),  // 10 PM
        (0,  3),  // 11 PM — winding down
    };

    // ── Trip spawn probability by hour (% chance per hour-tick to try spawning) ──
    private static readonly int[] _spawnChanceByHour = new int[24]
    {
         0,  0,  0,  0,  5, 15,   // 12AM–5AM
        30, 50, 50, 25, 15, 20,   //  6AM–11AM
        30, 20, 10, 10, 25, 50,   // 12PM–5PM
        50, 30, 15,  5,  2,  0,   //  6PM–11PM
    };

    // ── Admin op probability by hour (% chance per hour-tick) ─────────────
    private static readonly int[] _adminChanceByHour = new int[24]
    {
         5,  5,  5,  5,  5,  5,   // 12AM–5AM  — almost nothing
        10, 5,   5, 15, 25, 30,   //  6AM–11AM
        20, 30, 40, 40, 20, 10,   // 12PM–5PM  — lull = admin time
        10, 20, 30, 20, 10,  5,   //  6PM–11PM
    };

    public SimulationService(
        IDriverService driverService,
        IJeepService jeepService,
        ILocationService locationService,
        IRouteService routeService,
        ITripService tripService)
    {
        _driverService = driverService;
        _jeepService = jeepService;
        _locationService = locationService;
        _routeService = routeService;
        _tripService = tripService;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  PUBLIC ENTRY POINT
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Runs a full simulated 24-hour day.
    /// 1 real second ≈ 1 simulated hour. Total runtime ≈ 24 real seconds.
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           AUTOMATION SERVICE — STARTING                  ║");
        Console.WriteLine("║   Scale: 1 real second = 1 simulated hour                ║");
        Console.WriteLine("║   Duration: 24 real seconds (full simulated day)         ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝\n");

        _simHour = 0;
        _tickCount = 0;

        for (int tick = 0; tick < TotalTicks; tick++)
        {
            if (cancellationToken.IsCancellationRequested) break;

            // Advance simulated clock every 1000 ticks
            int newHour = tick / TicksPerHour;
            if (newHour != _simHour)
            {
                _simHour = newHour;
                Console.WriteLine($"\n[AUTO] ⏰ ── {_simHour:D2}:00 ──────────────────────────────────────────");
            }

            await ProcessTickAsync();

            await Task.Delay(1, cancellationToken);
        }

        // Wind down: complete all remaining ongoing trips
        await WindDownAsync();

        Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           AUTOMATION SERVICE — DAY COMPLETE ✓            ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝\n");
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  TICK PROCESSOR
    // ═══════════════════════════════════════════════════════════════════════

    private async Task ProcessTickAsync()
    {
        // ── 1. Always: drive ALL ongoing trips one step forward ────────────
        // This is the core — every tick, every active trip advances its FSM.
        await DriveOngoingTripsAsync();

        // ── 2. Maybe: try to spawn a new trip (based on time-of-day odds) ──
        int spawnRoll = _rng.Next(TicksPerHour); // rolls 0–999 each tick
        if (spawnRoll < _spawnChanceByHour[_simHour])
            await TrySpawnTripAsync();

        // ── 3. Maybe: do an admin operation ───────────────────────────────
        int adminRoll = _rng.Next(TicksPerHour);
        if (adminRoll < _adminChanceByHour[_simHour])
            await DoAdminOperationAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  DRIVE ONGOING TRIPS FORWARD
    //
    //  Calls NextStop once per ongoing trip per tick.
    //  NextStop's FSM handles all the sequencing internally — we just keep
    //  feeding it until the trip is no longer OnGoing (it auto-completes
    //  when it logs Arrival at the terminal end location).
    // ═══════════════════════════════════════════════════════════════════════

    private async Task DriveOngoingTripsAsync()
    {
        List<TripDto> allTrips;
        try { allTrips = await _tripService.GetTripsAsync(); }
        catch { return; }

        var ongoing = allTrips
            .Where(t => t.Status == nameof(TripStatus.OnGoing))
            .ToList();

        foreach (var trip in ongoing)
        {
            var (minPax, maxPax) = _paxByHour[_simHour];
            int passengers = _rng.Next(minPax, maxPax + 1);

            try
            {
                await _tripService.NextStop(new NextStopRequest
                {
                    TripId = trip.Id,
                    PassengerCount = passengers
                });
                Console.WriteLine($"[AUTO] NextStop → Trip [{trip.Id}] {trip.RouteCode} | {passengers} pax | {_simHour:D2}:00");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AUTO] NextStop failed for trip [{trip.Id}]: {ex.Message}");
            }
        }

        // Also start any Waiting trips that haven't been dispatched yet
        var waiting = allTrips
            .Where(t => t.Status == nameof(TripStatus.Waiting))
            .ToList();

        foreach (var trip in waiting)
        {
            try
            {
                await _tripService.StartTrip(trip.Id);
                Console.WriteLine($"[AUTO] Dispatched waiting trip [{trip.Id}] at {_simHour:D2}:00");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AUTO] Could not dispatch trip [{trip.Id}]: {ex.Message}");
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  SPAWN TRIP
    //
    //  Finds an available driver + their assigned jeep (with a route),
    //  creates and immediately starts the trip.
    //  Alternates direction based on the driver's last trip.
    // ═══════════════════════════════════════════════════════════════════════

    private async Task TrySpawnTripAsync()
    {
        List<DriverDto> drivers;
        List<JeepneyDto> jeeps;

        try
        {
            drivers = await _driverService.GetAsync();
            jeeps = await _jeepService.GetAsync();
        }
        catch { return; }

        // Find a jeep that has a route, is not currently on a trip, and has a driver
        var allTrips = await _tripService.GetTripsAsync();
        var busyJeepIds = allTrips
            .Where(t => t.Status == nameof(TripStatus.OnGoing) || t.Status == nameof(TripStatus.Waiting))
            .Select(t => t.Id) // TripDto doesn't expose JeepneyId directly, use plate as proxy below
            .ToHashSet();

        // Get jeeps with routes that aren't actively tripping
        var eligibleJeeps = jeeps
            .Where(j => !string.IsNullOrEmpty(j.RouteCode))   // has a route assigned
            .Where(j => j.DriverCount > 0)                     // has at least one driver
            .Where(j => !allTrips.Any(t =>
                t.PlateNumber == j.PlateNumber &&
                (t.Status == nameof(TripStatus.OnGoing) || t.Status == nameof(TripStatus.Waiting))))
            .ToList();

        if (eligibleJeeps.Count == 0) return;

        var jeep = eligibleJeeps[_rng.Next(eligibleJeeps.Count)];

        // Find a driver assigned to this jeep who isn't currently on a trip
        var jeepDrivers = await _jeepService.GetJeepneyDriversAsync(jeep.Id);
        var availableDriver = jeepDrivers.FirstOrDefault(d =>
            d.IsAvailable &&
            !allTrips.Any(t =>
                t.Status == nameof(TripStatus.OnGoing) &&
                t.PlateNumber == jeep.PlateNumber));

        if (availableDriver == null) return;

        // Alternate direction from last completed trip on this jeep
        var lastTrip = allTrips
            .Where(t => t.PlateNumber == jeep.PlateNumber && t.Status == nameof(TripStatus.Completed))
            .LastOrDefault();

        RouteDirection direction;
        if (lastTrip == null)
            direction = RouteDirection.Forward;
        else
            direction = lastTrip.RouteCode.Contains("Return") ? RouteDirection.Forward : RouteDirection.Return;

        try
        {
            await _tripService.CreateDriverTrip(new StartTripRequest
            {
                DriverId = availableDriver.Id,
                JeepId = jeep.Id,
                Direction = direction
            });

            // Fetch the just-created trip and start it immediately
            var fresh = await _tripService.GetTripsAsync();
            var newTrip = fresh
                .Where(t => t.PlateNumber == jeep.PlateNumber && t.Status == nameof(TripStatus.Waiting))
                .OrderByDescending(t => t.Id)
                .FirstOrDefault();

            if (newTrip != null)
            {
                await _tripService.StartTrip(newTrip.Id);
                Console.WriteLine($"[AUTO] 🚌 Trip [{newTrip.Id}] STARTED | Jeep {jeep.PlateNumber} | Driver [{availableDriver.Id}] {availableDriver.FirstName} {availableDriver.LastName} | {direction} | {_simHour:D2}:00");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AUTO] SpawnTrip failed: {ex.Message}");
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  ADMIN OPERATIONS
    //  Weighted so reads are most common, updates moderate, deletes rare.
    // ═══════════════════════════════════════════════════════════════════════

    private async Task DoAdminOperationAsync()
    {
        // Weights: Read=50, Update=35, Create=10, Delete=5
        int roll = _rng.Next(100);

        try
        {
            if (roll < 50) await DoReadAsync();
            else if (roll < 85) await DoUpdateAsync();
            else if (roll < 95) await DoCreateAsync();
            else await DoDeleteAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AUTO] Admin op failed: {ex.Message}");
        }
    }

    private async Task DoReadAsync()
    {
        int pick = _rng.Next(4);
        switch (pick)
        {
            case 0:
                var drivers = await _driverService.GetAsync();
                Console.WriteLine($"[AUTO] READ: Driver list ({drivers.Count} drivers)");
                if (drivers.Count > 0)
                {
                    var d = drivers[_rng.Next(drivers.Count)];
                    await _driverService.GetDetail(d.Id);
                    Console.WriteLine($"[AUTO] READ: Driver detail [{d.Id}] {d.FirstName} {d.LastName}");
                }
                break;

            case 1:
                var jeeps = await _jeepService.GetAsync();
                Console.WriteLine($"[AUTO] READ: Jeep list ({jeeps.Count} jeeps)");
                if (jeeps.Count > 0)
                {
                    var j = jeeps[_rng.Next(jeeps.Count)];
                    await _jeepService.GetDetail(j.Id);
                    Console.WriteLine($"[AUTO] READ: Jeep detail [{j.Id}] {j.PlateNumber}");
                }
                break;

            case 2:
                var routes = await _routeService.GetRoutesAsync();
                Console.WriteLine($"[AUTO] READ: Route list ({routes.Count} routes)");
                if (routes.Count > 0)
                {
                    var r = routes[_rng.Next(routes.Count)];
                    await _routeService.GetDetailAsync(r.Id);
                    Console.WriteLine($"[AUTO] READ: Route detail [{r.Id}] {r.RouteCode}");
                }
                break;

            case 3:
                var trips = await _tripService.GetTripsAsync();
                Console.WriteLine($"[AUTO] READ: Trip list ({trips.Count} trips)");
                if (trips.Count > 0)
                {
                    var t = trips[_rng.Next(trips.Count)];
                    await _tripService.GetDetailAsync(t.Id);
                    Console.WriteLine($"[AUTO] READ: Trip detail [{t.Id}] {t.RouteCode} — {t.LogCount} logs");
                }
                break;
        }
    }

    private async Task DoUpdateAsync()
    {
        int pick = _rng.Next(3);
        switch (pick)
        {
            case 0:
                var drivers = await _driverService.GetAsync();
                if (drivers.Count == 0) return;
                var driver = drivers[_rng.Next(drivers.Count)];
                await _driverService.UpdateAsync(new UpdateDriverRequest
                {
                    Id = driver.Id,
                    FirstName = driver.FirstName,
                    LastName = driver.LastName,
                    LicenseNumber = driver.LicenseNumber,
                    ContactNumber = GeneratePhoneNumber()
                });
                Console.WriteLine($"[AUTO] UPDATE: Driver [{driver.Id}] contact number refreshed");
                break;

            case 1:
                var jeeps = await _jeepService.GetAsync();
                var routes = await _routeService.GetRoutesAsync();
                if (jeeps.Count == 0 || routes.Count == 0) return;
                var jeep = jeeps[_rng.Next(jeeps.Count)];
                var route = routes[_rng.Next(routes.Count)];
                // Only update jeeps not currently on a trip
                var trips = await _tripService.GetTripsAsync();
                bool jeepBusy = trips.Any(t =>
                    t.PlateNumber == jeep.PlateNumber &&
                    (t.Status == nameof(TripStatus.OnGoing) || t.Status == nameof(TripStatus.Waiting)));
                if (jeepBusy) return;
                await _jeepService.UpdateAsync(new UpdateJeepneyRequest
                {
                    Id = jeep.Id,
                    PlateNumber = jeep.PlateNumber,
                    BodyNumber = jeep.BodyNumber,
                    Capacity = jeep.Capacity,
                    RouteId = route.Id
                });
                Console.WriteLine($"[AUTO] UPDATE: Jeep [{jeep.Id}] re-routed to {route.RouteCode}");
                break;

            case 2:
                var locs = await _locationService.GetAsync();
                if (locs.Count == 0) return;
                var loc = locs[_rng.Next(locs.Count)];
                await _locationService.UpdateAsync(new UpdateLocationRequest
                {
                    Id = loc.Id,
                    Name = loc.Name,
                    Description = $"Updated stop info — {_simHour:D2}:00"
                });
                Console.WriteLine($"[AUTO] UPDATE: Location [{loc.Id}] {loc.Name} description updated");
                break;
        }
    }

    private async Task DoCreateAsync()
    {
        // Only create new drivers during the day (realistic: new hires processed during office hours)
        if (_simHour < 8 || _simHour > 17) return;

        string[] firstNames = { "Rodolfo", "Ernesto", "Florencio", "Jacinto", "Hermogenes", "Tranquilino" };
        string[] lastNames = { "Ilustrisimo", "Ybañez", "Cuenco", "Vestil", "Bacalso", "Escario" };

        string fn = firstNames[_rng.Next(firstNames.Length)];
        string ln = lastNames[_rng.Next(lastNames.Length)];

        await _driverService.CreateAsync(new CreateDriverRequest
        {
            FirstName = fn,
            LastName = ln,
            LicenseNumber = GenerateRandomLicense(),
            ContactNumber = GeneratePhoneNumber(),
            DateHired = DateTime.UtcNow
        });
        Console.WriteLine($"[AUTO] CREATE: New driver {fn} {ln} hired at {_simHour:D2}:00");
    }

    private async Task DoDeleteAsync()
    {
        // Only archive completed trips — safest delete with least side effects
        var trips = await _tripService.GetTripsAsync();
        var completed = trips.Where(t => t.Status == nameof(TripStatus.Completed)).ToList();
        if (completed.Count == 0) return;

        var trip = completed[_rng.Next(completed.Count)];
        await _tripService.DeleteAsync(trip.Id);
        Console.WriteLine($"[AUTO] DELETE: Archived completed trip [{trip.Id}]");
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  WIND DOWN — drive all remaining ongoing trips to completion
    // ═══════════════════════════════════════════════════════════════════════

    private async Task WindDownAsync()
    {
        Console.WriteLine("\n[AUTO] ── Winding down: completing all remaining trips ──");

        for (int safety = 0; safety < 200; safety++)
        {
            var trips = await _tripService.GetTripsAsync();
            var ongoing = trips.Where(t => t.Status == nameof(TripStatus.OnGoing)).ToList();
            var waiting = trips.Where(t => t.Status == nameof(TripStatus.Waiting)).ToList();

            foreach (var t in waiting)
            {
                try { await _tripService.StartTrip(t.Id); } catch { }
            }

            if (ongoing.Count == 0 && waiting.Count == 0) break;

            foreach (var trip in ongoing)
            {
                try
                {
                    await _tripService.NextStop(new NextStopRequest
                    {
                        TripId = trip.Id,
                        PassengerCount = _rng.Next(0, 5)
                    });
                }
                catch { }
            }
        }

        Console.WriteLine("[AUTO] Wind-down complete.");
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  HELPERS
    // ═══════════════════════════════════════════════════════════════════════

    private static string GenerateRandomLicense()
    {
        return $"{(char)('A' + _rng.Next(26))}{_rng.Next(10)}{_rng.Next(10)}-" +
               $"{_rng.Next(10)}{_rng.Next(10)}-{_rng.Next(100000, 999999)}";
    }

    private static string GeneratePhoneNumber()
    {
        string[] prefixes = { "0917", "0927", "0932", "0947", "0961", "0966", "0977", "0995" };
        return $"{prefixes[_rng.Next(prefixes.Length)]}{_rng.Next(1000000, 9999999):D7}";
    }
}