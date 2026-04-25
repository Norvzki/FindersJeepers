
public interface IOptionService
{
    Task<List<DriverOption>> GetDriversForJeep(int jeepId);
    Task<List<JeepneyOption>> GetJeepsForDriver(int driverId);
}