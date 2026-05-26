public interface IGenerationService
{
    Task GenerateAsync(int driverCount = -1, int jeepCount = -1, int locationCount = -1, int routeCount = -1);
}