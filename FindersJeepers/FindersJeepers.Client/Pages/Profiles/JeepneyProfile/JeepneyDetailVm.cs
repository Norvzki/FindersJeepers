using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

public class JeepneyDetailViewModel
{
    private readonly HttpClient _http;
    private readonly ISnackbar _snackbar;
    private readonly NavigationManager nav;

    // ── Data ──
    public int JeepneyId { get; private set; }
    public JeepneyDetail? Jeepney { get; private set; }
    public List<BreadcrumbItem> Breadcrumbs { get; private set; } = new();

    // ── Computed status ──
    public string JeepneyStatus =>
        Jeepney?.RouteCode is "" ? "Unavailable" :
        Jeepney?.CurrentTrip is null ? "Available" :
        Jeepney.CurrentTrip.Status == "Waiting" ? "Waiting" : "On a Trip";

    public Color JeepneyStatusColor =>
        JeepneyStatus switch
        {
            "Available" => Color.Info,
            "Unavailable" => Color.Error,
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

    // Edit Driver Dialog
    public bool EditJeepneyVisible { get; set; }
    public JeepneyForm Form { get; set; } = new();
    public List<RouteSummary> _availableRoutes { get; set; } = new();

    public JeepneyDetailViewModel(HttpClient http, ISnackbar snackbar, NavigationManager nav)
    {
        _http = http;
        _snackbar = snackbar;
        this.nav = nav;
    }

    public async Task InitializeAsync(int jeepneyId)
    {
        JeepneyId = jeepneyId;
        await LoadDataAsync();
    }

    public async Task LoadDataAsync()
    {
        Jeepney = await _http.GetFromJsonOrRedirectAsync<JeepneyDetail>($"/api/v1/jeepneys/{JeepneyId}", nav);
        Breadcrumbs = new List<BreadcrumbItem>
        {
            new("Jeepneys", href: "/jeepneys"),
            new(Jeepney!.PlateNumber, href: null, disabled: true)
        };
        _availableRoutes = await _http.GetFromJsonAsync<List<RouteSummary>>("/api/v1/routes/");
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
            var errorMessage = await response.Content.ReadAsStringAsync();
            _snackbar.Add(errorMessage);
        }
    }

    // ── Manage Drivers ──
    public void OpenManageDriversDialog() => ManageDriversVisible = true;
    public void CloseManageDriversDialog() => ManageDriversVisible = false;

    public async Task ConfirmRemoveDriverAsync(JeepneyDriverDto driver)
    {
        var response = await _http.DeleteAsync($"/api/v1/jeepneys/{JeepneyId}/drivers/{driver.Id}");

        if(response.IsSuccessStatusCode)
        {
            _snackbar.Add("Removed jeepney drivers!");
        }
        else
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            _snackbar.Add(errorMessage);
        }

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
            var errorMessage = await response.Content.ReadAsStringAsync();
            _snackbar.Add(errorMessage);
        }
    }

    // Edit Driver Form
    public void OpenEditJeepneyDialog()
    {
        EditJeepneyVisible = true;
        Form = new JeepneyForm
        {
            Id = Jeepney.Id,
            BodyNumber = Jeepney.BodyNumber,
            Capacity = Jeepney.Capacity,
            PlateNumber = Jeepney.PlateNumber,
            RouteId = Jeepney.RouteCode == string.Empty ? null : _availableRoutes.Where(x => x.RouteCode == Jeepney.RouteCode).Select(x => x.Id).FirstOrDefault()
        };
    }
    public void CloseEditJeepneyDialog() => EditJeepneyVisible = false;

    public async Task OnSaveJeepneyAsync()
    {
        var payload = new UpdateJeepneyRequest
        {
            Id = Form.Id,
            BodyNumber = Form.BodyNumber,
            Capacity = Form.Capacity,
            PlateNumber = Form.PlateNumber,
            RouteId = Form.RouteId
        };

        var response = await _http.PutAsJsonAsync($"/api/v1/jeepneys/{JeepneyId}", payload);

        if(response.IsSuccessStatusCode)
        {
            _snackbar.Add("Successfully updated Jeepney!");
            CloseEditJeepneyDialog();
            await LoadDataAsync();
        }
        else
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            _snackbar.Add(errorMessage);
        }
    }

}