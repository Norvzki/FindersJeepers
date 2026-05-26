using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Generates randomized but realistic seed data for all entities (Drivers, Jeepneys, Locations, Routes).
/// Calling GenerateAsync() wipes everything first and then rebuilds from scratch.
/// Trips are intentionally excluded — the AutomationService handles trip lifecycle.
/// </summary>
public class GenerationService : IGenerationService
{
    private readonly IDriverService _driverService;
    private readonly IJeepService _jeepService;
    private readonly ILocationService _locationService;
    private readonly IRouteService _routeService;
    private readonly ITripService _tripService;

    private static readonly Random _rng = new();

    // ── Cebu-flavored name pools ────────────────────────────────────────────
    private static readonly string[] _firstNames =
    {
        "Juan", "Pedro", "Jose", "Mario", "Eduardo", "Rommel", "Danilo",
        "Fernando", "Roberto", "Antonio", "Carlo", "Mark", "John", "Ryan",
        "Jayson", "Rodel", "Noel", "Gilbert", "Renato", "Arnel", "Leo",
        "Michael", "Dennis", "Raul", "Efren", "Marvin", "Lito", "Nonoy",
        "Bong", "Rex", "Alvin", "Vergel", "Edgar", "Melvin", "Jomar"
    };

    private static readonly string[] _lastNames =
    {
        "Santos", "Reyes", "Cruz", "Bautista", "Ocampo", "Garcia", "Torres",
        "Dela Cruz", "Flores", "Villanueva", "Gonzales", "Mendoza", "Ramos",
        "Aquino", "Castillo", "Soriano", "Lim", "Tan", "Uy", "Sy",
        "Cabrera", "Fuentes", "Navarro", "Padilla", "Aguilar", "Celestino",
        "Valdez", "Miranda", "Espinosa", "Catalan", "Abella", "Pepito",
        "Ouano", "Cañete", "Labella"
    };

    // Cebu City real barangay/landmark locations
    private static readonly (string Name, string Description)[] _locationPool =
    {
        ("Carbon Market",       "Main public market in Cebu City; major jeepney terminal hub"),
        ("SM City Cebu",        "Large shopping mall near the North Reclamation Area"),
        ("Ayala Center Cebu",   "Upscale commercial center in Cebu Business Park"),
        ("Colon Street",        "Oldest street in the Philippines; downtown Cebu"),
        ("IT Park",             "Cebu IT Park; BPO district and commercial area"),
        ("UC Main Campus",      "University of Cebu main campus, Sanciangko St."),
        ("USC Main Campus",     "University of San Carlos main campus, P. del Rosario St."),
        ("Talamban",            "Northeastern residential and commercial district"),
        ("Talisay City Hall",   "Talisay City government center, south of Cebu City"),
        ("Mandaue City Hall",   "Mandaue City government center, north of Cebu City"),
        ("Mactan Island",       "Island connected by the Mactan-Cebu Bridge"),
        ("Lapu-Lapu City",      "City on Mactan Island; site of the Battle of Mactan"),
        ("North Bus Terminal",  "NBTC — northern inter-city bus terminal"),
        ("South Bus Terminal",  "SBTC — southern inter-city bus terminal"),
        ("Pier 1 Cebu",         "Main passenger port in Cebu City"),
        ("Robinson's Galleria", "Shopping mall on General Maxilom Ave."),
        ("Banawa",              "Residential and commercial barangay, western Cebu City"),
        ("Basak Pardo",         "Southern district barangay along the coastal road"),
        ("Mambaling",           "Populous coastal barangay, southwest Cebu City"),
        ("Bulacao",             "Junction area; southern boundary of Cebu City"),
        ("Guadalupe",           "Mid-city residential barangay"),
        ("Lahug",               "Uptown district; government offices and residences"),
        ("Apas",                "Barangay adjacent to IT Park and Camp Lapu-Lapu"),
        ("Gaisano Country Mall","Commercial mall in Banilad, Cebu City"),
        ("Danao City",          "Northern Cebu city; industrial port area"),
        ("Toledo City",         "Western Cebu coast; copper mining hub"),
        ("Naga City",           "South Cebu; industrial and residential town"),
        ("Minglanilla",         "Rapidly urbanizing municipality south of Cebu City"),
        ("Consolacion",         "Northern municipality adjacent to Mandaue"),
        ("Liloan",              "Coastal municipality north of Consolacion")
    };

    // Real Cebu jeepney route codes
    private static readonly string[] _routeCodePool =
    {
        "01A", "01B", "02A", "02C", "04A", "04B", "06B", "07B",
        "10C", "10D", "17C", "17D", "20C", "20D", "23A", "23B",
        "GV",  "IT",  "SM",  "AY",  "CB",  "MC",  "03B", "05A",
        "08C", "09D", "11A", "12B", "13C", "14D"
    };

    public GenerationService(
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
    /// Clears ALL existing data and regenerates everything with random but
    /// coherent values. Pass -1 (or omit) any count to let the service pick
    /// a sensible random number.
    /// </summary>
    public async Task GenerateAsync(
        int driverCount = -1,
        int jeepCount = -1,
        int locationCount = -1,
        int routeCount = -1)
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              GENERATION SERVICE — START                  ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

        // ── 1. Resolve counts ──────────────────────────────────────────────
        int drivers = driverCount > 0 ? driverCount : _rng.Next(10, 31);
        int jeeps = jeepCount > 0 ? jeepCount : _rng.Next(8, 21);
        int locations = locationCount > 0 ? locationCount : _rng.Next(12, 25);
        int routes = routeCount > 0 ? routeCount : _rng.Next(4, 12);

        // Clamp to pool sizes
        locations = Math.Min(locations, _locationPool.Length);
        routes = Math.Min(routes, _routeCodePool.Length);

        // Routes need at least 2 locations for start/end + at least 1 for intermediate stops
        // Enforce: locations >= routes * 1 as a bare minimum (we need variety for intermediates)
        if (locations < 4)
        {
            locations = 4;
            Console.WriteLine("[GEN] WARNING: location count raised to 4 (minimum needed for routes + stops).");
        }

        Console.WriteLine($"\n[GEN] Resolved counts → Drivers: {drivers} | Jeeps: {jeeps} | Locations: {locations} | Routes: {routes}");

        // ── 2. Wipe existing data ──────────────────────────────────────────
        await PurgeAllAsync();

        // ── 3. Create entities in dependency order ─────────────────────────
        var locationIds = await CreateLocationsAsync(locations);
        var routeIds = await CreateRoutesAsync(routes, locationIds);
        var driverIds = await CreateDriversAsync(drivers);
        var jeepIds = await CreateJeepsAsync(jeeps, routeIds);

        // ── 4. Wire up drivers ↔ jeepneys ─────────────────────────────────
        await AssignDriversToJeepsAsync(driverIds, jeepIds);

        Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           GENERATION SERVICE — COMPLETE ✓                ║");
        Console.WriteLine($"║  {drivers} drivers | {jeeps} jeeps | {locations} locations | {routes} routes");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝\n");
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  PURGE
    //  Order matters — services enforce these constraints:
    //    • Trip.Delete()       → only allowed if Status == Completed
    //    • Jeep.Delete()       → blocked if jeep is currently on a trip
    //    • Driver.Delete()     → blocked if driver is currently on a trip
    //    • Route.Delete()      → blocked if active trips exist on it; also clears jeep routes
    //    • Location.Delete()   → blocked if any non-deleted route references it
    //
    //  Safe purge order:
    //    1. Complete all ongoing trips (so they become deletable)
    //    2. Delete all trips
    //    3. Delete all jeepneys (no active trip blocks anymore)
    //    4. Delete all drivers  (no active trip blocks anymore)
    //    5. Delete all routes   (no active trips; clears jeep routes internally — jeeps already gone)
    //    6. Delete all locations (no routes reference them anymore)
    // ═══════════════════════════════════════════════════════════════════════

    private async Task PurgeAllAsync()
    {
        Console.WriteLine("\n[GEN] ── Purging existing data ──");

        // 1. Force-complete any ongoing trips so they become deletable
        var allTrips = await _tripService.GetTripsAsync();
        foreach (var trip in allTrips.Where(t => t.Status == TripStatus.OnGoing.ToString()))
        {
            await _tripService.CompleteTrip(trip.Id);
            Console.WriteLine($"[GEN]   Force-completed ongoing trip #{trip.Id} before purge.");
        }

        // 2. Delete all trips (only Completed trips are deletable)
        var deletableTrips = await _tripService.GetTripsAsync();
        foreach (var trip in deletableTrips)
        {
            try
            {
                await _tripService.DeleteAsync(trip.Id);
                Console.WriteLine($"[GEN]   Deleted trip #{trip.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GEN]   Could not delete trip #{trip.Id}: {ex.Message}");
            }
        }

        // 3. Delete all jeepneys
        var jeeps = await _jeepService.GetAsync();
        foreach (var jeep in jeeps)
        {
            await _jeepService.DeleteAsync(jeep.Id);
            Console.WriteLine($"[GEN]   Deleted jeep #{jeep.Id} ({jeep.PlateNumber})");
        }

        // 4. Delete all drivers
        var drivers = await _driverService.GetAsync();
        foreach (var driver in drivers)
        {
            await _driverService.DeleteAsync(driver.Id);
            Console.WriteLine($"[GEN]   Deleted driver #{driver.Id} ({driver.FirstName} {driver.LastName})");
        }

        // 5. Delete all routes
        var routes = await _routeService.GetRoutesAsync();
        foreach (var route in routes)
        {
            await _routeService.DeleteAsync(route.Id);
            Console.WriteLine($"[GEN]   Deleted route #{route.Id} ({route.RouteCode})");
        }

        // 6. Delete all locations (routes are gone now, so no FK blocks)
        var locations = await _locationService.GetAsync();
        foreach (var loc in locations)
        {
            await _locationService.DeleteAsync(loc.Id);
            Console.WriteLine($"[GEN]   Deleted location #{loc.Id} ({loc.Name})");
        }

        Console.WriteLine("[GEN] Purge complete.\n");
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  LOCATIONS
    // ═══════════════════════════════════════════════════════════════════════

    private async Task<List<int>> CreateLocationsAsync(int count)
    {
        Console.WriteLine($"[GEN] ── Creating {count} locations ──");

        var shuffled = _locationPool.OrderBy(_ => _rng.Next()).Take(count).ToList();
        var ids = new List<int>();

        foreach (var (name, desc) in shuffled)
        {
            await _locationService.CreateAsync(new CreateLocationRequest
            {
                Name = name,
                Description = desc
            });

            // Fetch back to get the assigned ID
            var all = await _locationService.GetAsync();
            var created = all.OrderByDescending(l => l.Id).First();
            ids.Add(created.Id);
            Console.WriteLine($"[GEN]   Location created: [{created.Id}] {name}");
        }

        return ids;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  ROUTES
    //
    //  Invariants respected:
    //    • Route code must be unique across non-deleted routes
    //    • LocationStartId and LocationEndId must be valid (>= 1)
    //    • RouteStop.LocationId cannot equal the route's own StartLocation or EndLocation
    //      (AddRouteStopsAsync enforces this — so intermediates must be STRICTLY between)
    //    • AutoGenerateReturnRouteAsync requires at least one forward stop already saved
    // ═══════════════════════════════════════════════════════════════════════

    private async Task<List<int>> CreateRoutesAsync(int count, List<int> locationIds)
    {
        Console.WriteLine($"\n[GEN] ── Creating {count} routes ──");

        if (locationIds.Count < 3)
        {
            // Need start + end + at least 1 intermediate stop
            Console.WriteLine("[GEN]   Not enough locations to create routes with stops. Skipping.");
            return new List<int>();
        }

        var usedCodes = new HashSet<string>();
        var shuffledCodes = _routeCodePool.OrderBy(_ => _rng.Next()).ToList();
        var ids = new List<int>();
        int codeIndex = 0;

        for (int i = 0; i < count; i++)
        {
            if (codeIndex >= shuffledCodes.Count)
            {
                Console.WriteLine("[GEN]   Ran out of unique route codes. Stopping route creation.");
                break;
            }

            // Pick two DISTINCT locations for start/end
            var shuffledLocs = locationIds.OrderBy(_ => _rng.Next()).Take(2).ToList();
            int startId = shuffledLocs[0];
            int endId = shuffledLocs[1];

            string code = shuffledCodes[codeIndex++];

            await _routeService.CreateRouteAsync(new CreateRouteRequest
            {
                RouteCode = code,
                StartLocation = startId,
                EndLocation = endId
            });

            var all = await _routeService.GetRoutesAsync();
            var created = all.OrderByDescending(r => r.Id).First();
            ids.Add(created.Id);

            Console.WriteLine($"[GEN]   Route created: [{created.Id}] {code}  (loc {startId} → {endId})");

            // Add intermediate stops — MUST exclude startId and endId per domain rule
            bool stopsAdded = await AddRouteStopsAsync(created.Id, locationIds, startId, endId);

            // AutoGenerateReturnRoute requires forward stops to already exist
            if (stopsAdded && _rng.Next(100) < 60)
            {
                await _routeService.AutoGenerateReturnRouteAsync(created.Id);
                Console.WriteLine($"[GEN]   Return route auto-generated for route [{created.Id}] {code}");
            }
        }

        return ids;
    }

    /// <summary>
    /// Adds 1–3 intermediate stops to the route's Forward direction.
    /// Explicitly filters out the route's own start and end locations,
    /// which AddRouteStopsAsync would reject as a DomainException.
    /// Returns true if stops were successfully added.
    /// </summary>
    private async Task<bool> AddRouteStopsAsync(int routeId, List<int> allLocationIds, int startId, int endId)
    {
        // Intermediate pool = all locations EXCEPT start and end
        var intermediatePool = allLocationIds
            .Where(id => id != startId && id != endId)
            .OrderBy(_ => _rng.Next())
            .ToList();

        if (intermediatePool.Count == 0)
        {
            Console.WriteLine($"[GEN]   Route [{routeId}]: no intermediate locations available for stops. Skipping stops.");
            return false;
        }

        // Pick 1–3 intermediates (don't over-pick from a small pool)
        int stopCount = Math.Min(_rng.Next(1, 4), intermediatePool.Count);
        var stops = intermediatePool.Take(stopCount).ToList();

        // Assign sequential indices starting at 0 — ONLY the intermediates go here.
        // The route already knows its own start/end; AddRouteStopsAsync manages the middle.
        var pairs = stops.Select((locId, idx) => new LocationIndexPair
        {
            LocationId = locId,
            Index = idx
        }).ToList();

        await _routeService.AddRouteStopsAsync(new AddRouteStopRequest
        {
            RouteId = routeId,
            RouteStops = pairs,
            RouteDirection = RouteDirection.Forward
        });

        Console.WriteLine($"[GEN]   Route [{routeId}]: added {stopCount} intermediate stop(s).");
        return true;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  DRIVERS
    //
    //  Invariants respected:
    //    • LicenseNumber must be unique
    //    • DateHired must not be in the future
    //    • DateHired must not be DateTime.MinValue
    //    • We use DateTime.UtcNow-based dates to match Driver.Create's ToUniversalTime() call
    // ═══════════════════════════════════════════════════════════════════════

    private async Task<List<int>> CreateDriversAsync(int count)
    {
        Console.WriteLine($"\n[GEN] ── Creating {count} drivers ──");

        var usedLicenses = new HashSet<string>();
        var ids = new List<int>();

        for (int i = 0; i < count; i++)
        {
            string firstName = _firstNames[_rng.Next(_firstNames.Length)];
            string lastName = _lastNames[_rng.Next(_lastNames.Length)];
            string license = GenerateUniqueLicense(usedLicenses);

            await _driverService.CreateAsync(new CreateDriverRequest
            {
                FirstName = firstName,
                LastName = lastName,
                LicenseNumber = license,
                ContactNumber = GeneratePhoneNumber(),
                DateHired = RandomDateHired()   // always past, never MinValue
            });

            var all = await _driverService.GetAsync();
            var created = all.OrderByDescending(d => d.Id).First();
            ids.Add(created.Id);
            Console.WriteLine($"[GEN]   Driver created: [{created.Id}] {firstName} {lastName}  License: {license}");
        }

        return ids;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  JEEPNEYS
    //
    //  Invariants respected:
    //    • PlateNumber must be unique
    //    • BodyNumber must be unique
    //    • Capacity must be between 8 and 40 (domain: "What kind of jeep are we making here?" / "not a bus")
    //    • RouteId, if set, must be >= 1 — we only pass IDs from the just-created routes list,
    //      never 0 or negative. Null is safe and left as-is.
    // ═══════════════════════════════════════════════════════════════════════

    private async Task<List<int>> CreateJeepsAsync(int count, List<int> routeIds)
    {
        Console.WriteLine($"\n[GEN] ── Creating {count} jeepneys ──");

        var usedPlates = new HashSet<string>();
        var usedBodies = new HashSet<string>();
        var ids = new List<int>();

        for (int i = 0; i < count; i++)
        {
            string plate = GenerateUniquePlate(usedPlates);
            string body = GenerateUniqueBodyNumber(usedBodies);
            int capacity = new[] { 16, 18, 20, 22, 24 }[_rng.Next(5)]; // all safely within 8–40

            // Assign a route 75% of the time, but only if we actually have routes
            int? routeId = routeIds.Count > 0 && _rng.Next(100) < 75
                           ? routeIds[_rng.Next(routeIds.Count)]
                           : (int?)null;

            await _jeepService.CreateAsync(new CreateJeepneyRequest
            {
                PlateNumber = plate,
                BodyNumber = body,
                Capacity = capacity,
                RouteId = routeId
            });

            var all = await _jeepService.GetAsync();
            var created = all.OrderByDescending(j => j.Id).First();
            ids.Add(created.Id);
            Console.WriteLine($"[GEN]   Jeep created: [{created.Id}] Plate: {plate}  Body: {body}  Cap: {capacity}  Route: {routeId?.ToString() ?? "unassigned"}");
        }

        return ids;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  DRIVER ↔ JEEPNEY ASSIGNMENTS
    //
    //  Invariants respected:
    //    • AssignDriversAsync skips if the driver is already an active driver of that jeep
    //      (Jeepney.AssignDriver throws "already assigned"). The service itself guards this,
    //      but we also queue drivers to avoid repeat assignments within our own loop.
    // ═══════════════════════════════════════════════════════════════════════

    private async Task AssignDriversToJeepsAsync(List<int> driverIds, List<int> jeepIds)
    {
        Console.WriteLine("\n[GEN] ── Assigning drivers to jeepneys ──");

        if (driverIds.Count == 0 || jeepIds.Count == 0) return;

        // Shuffle drivers and distribute them. Each driver goes into the queue once,
        // so no driver is double-assigned to two different jeeps in the same pass.
        var availableDrivers = new Queue<int>(driverIds.OrderBy(_ => _rng.Next()));

        foreach (int jeepId in jeepIds)
        {
            if (availableDrivers.Count == 0) break;

            var toAssign = new List<int>();
            toAssign.Add(availableDrivers.Dequeue());

            // 40% chance of a reliever driver
            if (availableDrivers.Count > 0 && _rng.Next(100) < 40)
                toAssign.Add(availableDrivers.Dequeue());

            await _jeepService.AssignDriversAsync(new AssignDriversRequest
            {
                JeepId = jeepId,
                DriverIds = toAssign
            });

            Console.WriteLine($"[GEN]   Jeep [{jeepId}] ← Drivers {string.Join(", ", toAssign.Select(d => $"[{d}]"))}");
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  HELPERS
    // ═══════════════════════════════════════════════════════════════════════

    private static string GenerateUniqueLicense(HashSet<string> used)
    {
        string license;
        do
        {
            license = $"{(char)('A' + _rng.Next(26))}{_rng.Next(10)}{_rng.Next(10)}-" +
                      $"{_rng.Next(10)}{_rng.Next(10)}-" +
                      $"{_rng.Next(100000, 999999)}";
        } while (used.Contains(license));
        used.Add(license);
        return license;
    }

    private static string GeneratePhoneNumber()
    {
        string[] prefixes =
        {
            "0917", "0918", "0919", "0920", "0926", "0927",
            "0932", "0933", "0935", "0936", "0939", "0947",
            "0948", "0949", "0955", "0961", "0966", "0967",
            "0973", "0975", "0977", "0994", "0995", "0997"
        };
        string prefix = prefixes[_rng.Next(prefixes.Length)];
        return $"{prefix}{_rng.Next(1000000, 9999999):D7}";
    }

    private static string GenerateUniquePlate(HashSet<string> used)
    {
        string plate;
        do
        {
            string letters = new string(new[]
            {
                (char)('A' + _rng.Next(26)),
                (char)('A' + _rng.Next(26)),
                (char)('A' + _rng.Next(26))
            });
            plate = $"{letters} {_rng.Next(1000, 9999)}";
        } while (used.Contains(plate));
        used.Add(plate);
        return plate;
    }

    private static string GenerateUniqueBodyNumber(HashSet<string> used)
    {
        string body;
        do { body = $"{_rng.Next(1000, 9999)}"; }
        while (used.Contains(body));
        used.Add(body);
        return body;
    }

    private static DateTime RandomDateHired()
    {
        // Between 6 months ago and 10 years ago — always in the past, never MinValue
        int daysAgo = _rng.Next(180, 365 * 10);
        return DateTime.UtcNow.AddDays(-daysAgo);
    }
}