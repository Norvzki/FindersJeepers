
/// <summary>
/// A service object mostly used for fetching option items for dropdowns.
/// Im not sure if this is good design.
/// </summary>
public interface IOptionService
{
    /// <summary>
    /// When assigning a driver to a jeep, we need to get a list of drivers who doesnt drive that jeep yet.
    /// This method takes care of it.
    /// </summary>
    /// <param name="jeepId"></param>
    /// <returns></returns>
    Task<List<DriverOption>> GetDriversForJeep(int jeepId);
    /// <summary>
    /// When assigning a jeep to a driver, we need to get a list of jeeps that is not driven by that jeep yet.
    /// This method takes care of it.
    /// </summary>
    /// <param name="driverId"></param>
    /// <returns></returns>
    Task<List<JeepneyOption>> GetJeepsForDriver(int driverId);
    /// <summary>
    /// This method is probably obselete. Might fix later.
    /// </summary>
    /// <returns></returns>
    Task<List<LocationDto>> GetLocations();
    /// <summary>
    /// Given a query, returns a list of locations that match/similar to that query.
    /// Mainly used for the locations list for adding routestops.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    Task<List<LocationDto>> SearchLocations(string query);
}