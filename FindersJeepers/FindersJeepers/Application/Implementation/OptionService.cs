
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

/// <summary>
/// // This service is responsible for the queries and processing of option DTOs.
/// </summary>
public class OptionService : IOptionService
{
    private readonly IUnitOfWork _uow;

    public OptionService(IUnitOfWork uow)
    {
        _uow = uow;
    }
    /// <summary>
    /// When assigning drivers to a jeep. We will need a list of drivers that are not currently drivers to that same jeep.
    /// </summary>
    public async Task<List<DriverOption>> GetDriversForJeep(int jeepId)
    {
        var jeep = await _uow.Jeepneys.GetByIdAsync(jeepId);
        var activeJeepDriverIds = jeep.Drivers.Where(p => p.UnassignedAt == null).Select(p => p.DriverId);

        var drivers = _uow.Drivers.Get();

        return await drivers.Where(d => !activeJeepDriverIds.Contains(d.Id))
            .Select(d => new DriverOption
            {
                Id = d.Id,
                FirstName = d.FirstName,
                LastName = d.LastName,
            })
            .ToListAsync();
    }
    /// <summary>
    /// When assigning jeeps to a driver. We will need a list of jeeps that are not currently driven by that driver.
    /// </summary>
    public async Task<List<JeepneyOption>> GetJeepsForDriver(int driverId)
    {

        return await (from j in _uow.Jeepneys.Get()
                      join r in _uow.Routes.Get() on j.RouteId equals r.Id
                      where j.Drivers.Any(x => x.DriverId == driverId)
                      select new JeepneyOption
                      {
                          Id = j.Id,
                          BodyNumber = j.BodyNumber,
                          Capacity = j.Capacity,
                          NumberOfDrivers = j.Drivers.Count(x => x.UnassignedAt == null),
                          PlateNumber = j.PlateNumber,
                          RouteCode = r.RouteCode
                      }
                      ).ToListAsync();

        //return await _uow.Jeepneys.Get()
        //    .Join(_uow.Routes.Get(), j => j.RouteId, r => r.Id)
        //    .Where(j => j.Drivers.Any(x => x.DriverId == driver.Id))
        //    .Select(j => new JeepneyOption
        //    {
        //        Id = j.Id,
        //        BodyNumber = j.BodyNumber,
        //        Capacity = j.Capacity,
        //        PlateNumber = j.PlateNumber,
        //        NumberOfDrivers = j.Drivers
        //        .Count(x=>x.UnassignedAt == null),
        //        RouteCode = 
        //    }).ToListAsync();
    }
} 
