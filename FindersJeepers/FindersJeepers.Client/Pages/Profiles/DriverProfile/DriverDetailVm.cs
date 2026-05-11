using MudBlazor;
using System.Net.Http.Json;

public class DriverDetailViewModel
{
    private readonly HttpClient _http;
    private readonly ISnackbar _snackbar;

    // ── Data ──
    public int DriverId { get; private set; }
    public DriverDetail? Driver { get; private set; }
    public List<BreadcrumbItem> Breadcrumbs { get; private set; } = new();

    // ── Computed ──
    public bool IsOnTrip =>
        Driver?.TripHistory.Any(t => t.Status is "OnGoing" or "Waiting") ?? false;

    public TripSummary? CurrentTrip =>
        Driver?.TripHistory.FirstOrDefault(t => t.Status is "OnGoing" or "Waiting");

    public IEnumerable<TripSummary> CompletedTrips =>
        Driver?.TripHistory.Where(t => t.Status == "Completed") ?? Enumerable.Empty<TripSummary>();

    public int YearsOfService =>
        Driver is null ? 0 : (int)((DateTime.Now - Driver.DateHired).TotalDays / 365);

    // ── Edit dialog ─
    public bool EditDialogVisible { get; set; }
    public DriverForm Form { get; set; } = new();

    // ── Start Trip dialog ──
    public bool StartTripVisible { get; set; }
    public List<JeepneySummary>? StartTripJeeps { get; private set; }
    public int? StartTripSelectedJeepId { get; set; }
    public RouteDirection StartTripDirection { get; set; } = RouteDirection.Forward;

    // ── Assign Jeepneys dialog ──
    public bool AssignJeepneysVisible { get; set; }
    public List<JeepneyOption>? AvailableJeepneys { get; private set; }
    public HashSet<int> SelectedJeepneyIds { get; set; } = new();

    public DriverDetailViewModel(HttpClient http, ISnackbar snackbar)
    {
        _http = http;
        _snackbar = snackbar;
    }

    public async Task InitializeAsync(int driverId)
    {
        DriverId = driverId;
        await Task.Delay(300);
        await LoadDataAsync();
        Breadcrumbs = new List<BreadcrumbItem>
        {
            new("Drivers", href: "/drivers"),
            new($"{Driver!.FirstName} {Driver.LastName}", href: null, disabled: true)
        };
    }

    public async Task LoadDataAsync()
    {
        Driver = await _http.GetFromJsonAsync<DriverDetail>($"/api/v1/drivers/{DriverId}");
    }

    // ── Start Trip ──
    public void OpenStartTripDialog()
    {
        StartTripSelectedJeepId = null;
        StartTripDirection = RouteDirection.Forward;
        StartTripJeeps = Driver?.AssignedJeepneys;
        StartTripVisible = true;
    }

    public void CloseStartTripDialog()
    {
        StartTripVisible = false;
        StartTripSelectedJeepId = null;
    }

    public async Task ConfirmStartTripAsync()
    {
        if (StartTripSelectedJeepId is null) return;

        var response = await _http.PostAsJsonAsync("/api/v1/trips", new StartTripRequest
        {
            JeepId = StartTripSelectedJeepId.Value,
            Direction = StartTripDirection,
            DriverId = DriverId
        });

        if (response.IsSuccessStatusCode)
        {
            _snackbar.Add("Trip started successfully!", Severity.Success);
            CloseStartTripDialog();
            await LoadDataAsync();
        }
        else
        {
            _snackbar.Add("Something went wrong.", Severity.Error);
        }
    }

    // ── Edit ──
    public void OpenEditDialog()
    {
        if (Driver is null) return;
        Form = new DriverForm
        {
            Id = Driver.Id,
            FirstName = Driver.FirstName,
            LastName = Driver.LastName,
            LicenseNumber = Driver.LicenseNumber,
            ContactNumber = Driver.ContactNumber,
            DateHired = Driver.DateHired
        };
        EditDialogVisible = true;
    }

    public void CloseEditDialog() => EditDialogVisible = false;

    public async Task SaveDriverAsync()
    {
        var response = await _http.PutAsJsonAsync($"/api/v1/drivers/{DriverId}", new UpdateDriverRequest
        {
            Id = Form.Id,
            FirstName = Form.FirstName,
            LastName = Form.LastName,
            ContactNumber = Form.ContactNumber,
            LicenseNumber = Form.LicenseNumber
        });

        if (response.IsSuccessStatusCode)
        {
            _snackbar.Add("Driver updated successfully!", Severity.Success);
            CloseEditDialog();
            await LoadDataAsync();
        }
        else
        {
            _snackbar.Add("Something went wrong.", Severity.Error);
        }
    }

    // ── Assign Jeepneys ──
    public async Task OpenAssignJeepneysDialogAsync()
    {
        SelectedJeepneyIds = Driver?.AssignedJeepneys?.Select(x => x.Id).ToHashSet() ?? new();
        AvailableJeepneys = null;
        AssignJeepneysVisible = true;
        AvailableJeepneys = await _http.GetFromJsonAsync<List<JeepneyOption>>(
            $"/api/v1/options/jeeps/available-for-driver/{DriverId}");
    }

    public void ToggleJeepney(int id)
    {
        if (!SelectedJeepneyIds.Add(id))
            SelectedJeepneyIds.Remove(id);
    }

    public void CloseAssignDialog()
    {
        AssignJeepneysVisible = false;
        SelectedJeepneyIds = new();
    }

    public async Task ConfirmAssignAsync()
    {
        var response = await _http.PutAsJsonAsync($"/api/v1/drivers/jeepneys/{DriverId}",
            new AssignJeepneysRequest { DriverId = DriverId, JeepIds = SelectedJeepneyIds.ToList() });

        if (response.IsSuccessStatusCode)
            _snackbar.Add("Assigned Jeepneys Successfully!", Severity.Success);
        else
            _snackbar.Add("Something went wrong...", Severity.Error);

        await LoadDataAsync();
        AssignJeepneysVisible = false;
        SelectedJeepneyIds = new();
    }

    // ── Nested form model ──
    public class DriverForm
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public DateTime? DateHired { get; set; }
    }
}