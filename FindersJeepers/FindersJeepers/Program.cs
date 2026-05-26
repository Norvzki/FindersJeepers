using FindersJeepers.Application;
using FindersJeepers.Client.Pages;
using FindersJeepers.Components;
using FindersJeepers.Infrastructure;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

var connectionString = builder.Configuration.GetConnectionString("JeepDB");


builder.Services.AddApplication()           // LFG YO?
    .AddInfrastructure();

builder.Services.AddScoped<DriverDetailViewModel>();
builder.Services.AddScoped<JeepneyDetailViewModel>();
builder.Services.AddScoped<LocationDetailViewModel>();

builder.Services.AddDbContext<MyDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7139/")
});

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseExceptionHandler();   // or your custom handler registration
app.UseStatusCodePages();    // optional, but keep it after
app.UseRouting();

app.UseHttpsRedirection();
app.MapControllers();
app.UseExceptionHandler();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(FindersJeepers.Client._Imports).Assembly);

app.Run();