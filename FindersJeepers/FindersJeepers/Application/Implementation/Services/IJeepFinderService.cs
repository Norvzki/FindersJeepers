public interface IJeepFinderService
{
    Task<List<FoundJeepDto>> FindJeepsAsync(int locationId);
}