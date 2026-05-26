public interface ISimulationService
{
    Task RunAsync(CancellationToken cancellationToken = default);
}