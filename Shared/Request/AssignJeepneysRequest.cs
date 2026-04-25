public record AssignJeepneysRequest
{
    public int DriverId { get; set; }
    public List<int> JeepIds { get; set; }
}
