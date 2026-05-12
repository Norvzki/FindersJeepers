using MudBlazor;
using System.Net.Http.Json;

public class JeepneyDetailViewModel
{
    private readonly HttpClient _http;
    private readonly ISnackbar _snackbar;

    // ── Data ──
    public int JeepneyId { get; private set; }
    public JeepneyDetail? Jeepney { get; private set; }
    public List<BreadcrumbItem> Breadcrumbs { get; private set; } = new();

    // ── Computed status ──
    public string JeepneyStatus =>
        Jeepney?.CurrentTrip is null ? "Available" :
        Jeepney.CurrentTrip.Status == "Waiting" ? "Waiting" : "On a Trip";

    public Color JeepneyStatusColor =>
        JeepneyStatus switch
        {
            "Available" => Color.Info,
            "Waiting" => Color.Warning,
            _ => Color.Success
        };

    public string JeepneyStatusIcon =>
        JeepneyStatus switch
        {
            "Available" => Icons.Material.Filled.Circle,
            "Waiting" => Icons.Material.Filled.HourglassTop,
            _ => Icons.Material.Filled.DirectionsCar
        };

    // ── Start Trip dialog ──
    public bool StartTripVisible { get; set; }
    public List<JeepneyDriverDto>? StartTripDrivers { get; private set; }
    public int? StartTripSelectedDriverId { get; set; }
    public RouteDirection StartTripDirection { get; set; } = RouteDirection.Forward;

    // ── Manage Drivers dialog ──
    public bool ManageDriversVisible { get; set; }

    // ── Assign Drivers dialog ──
    public bool AssignDriversVisible { get; set; }
    public List<DriverOption>? AvailableDrivers { get; private set; }
    public HashSet<int> SelectedDriverIds { get; set; } = new();

    private static readonly DialogOptions _sharedOptions = new()
    { MaxWidth = MaxWidth.Small, FullWidth = true, CloseOnEscapeKey = true };

    public DialogOptions DialogOptions => _sharedOptions;

    public const int PreviewCount = 3;

    public JeepneyDetailViewModel(HttpClient http, ISnackbar snackbar)
    {
        _http = http;
        _snackbar = snackbar;
    }

    public async Task InitializeAsync(int jeepneyId)
    {
        JeepneyId = jeepneyId;
        await LoadDataAsync();
    }

    public async Task LoadDataAsync()
    {
        Jeepney = await _http.GetFromJsonAsync<JeepneyDetail>($"/api/v1/jeepneys/{JeepneyId}");
        Breadcrumbs = new List<BreadcrumbItem>
        {
            new("Jeepneys", href: "/jeepneys"),
            new(Jeepney!.PlateNumber, href: null, disabled: true)
        };
    }

    // ── Start Trip ──
    public void OpenStartTripDialog()
    {
        StartTripSelectedDriverId = null;
        StartTripDirection = RouteDirection.Forward;
        StartTripDrivers = Jeepney?.AssignedDrivers;
        StartTripVisible = true;
    }

    public void CloseStartTripDialog()
    {
        StartTripVisible = false;
        StartTripSelectedDriverId = null;
    }

    public async Task ConfirmStartTripAsync()
    {
        if (StartTripSelectedDriverId is null) return;

        var response = await _http.PostAsJsonAsync("/api/v1/trips", new StartTripRequest
        {
            JeepId = JeepneyId,
            DriverId = StartTripSelectedDriverId.Value,
            Direction = StartTripDirection
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

    // ── Manage Drivers ──
    public void OpenManageDriversDialog() => ManageDriversVisible = true;
    public void CloseManageDriversDialog() => ManageDriversVisible = false;

    public async Task ConfirmRemoveDriverAsync(JeepneyDriverDto driver)
    {
        var response = await _http.DeleteAsync($"/api/v1/jeepneys/{JeepneyId}/drivers/{driver.Id}");

        _snackbar.Add(
            response.IsSuccessStatusCode ? $"{driver.FirstName} has been removed." : "Something went wrong.",
            response.IsSuccessStatusCode ? Severity.Info : Severity.Error);

        await LoadDataAsync();
    }

    // ── Assign Drivers ──
    public async Task OpenAssignDriversDialogAsync()
    {
        SelectedDriverIds = new();
        AvailableDrivers = null;
        AssignDriversVisible = true;
        AvailableDrivers = await _http.GetFromJsonAsync<List<DriverOption>>(
            $"/api/v1/options/drivers/available-for-jeep/{JeepneyId}");
    }

    public void CloseAssignDriversDialog() => AssignDriversVisible = false;

    public void ToggleDriver(int driverId)
    {
        if (!SelectedDriverIds.Add(driverId))
            SelectedDriverIds.Remove(driverId);
    }

    public async Task ConfirmAssignDriversAsync()
    {
        var response = await _http.PostAsJsonAsync($"/api/v1/jeepneys/{JeepneyId}/drivers",
            new AssignDriversRequest { JeepId = JeepneyId, DriverIds = SelectedDriverIds.ToList() });

        if (response.IsSuccessStatusCode)
        {
            _snackbar.Add("Drivers assigned successfully.", Severity.Success);
            AssignDriversVisible = false;
            SelectedDriverIds = new();
            await LoadDataAsync();
        }
        else
        {
            _snackbar.Add("Something went wrong...", Severity.Error);
        }
    }
}