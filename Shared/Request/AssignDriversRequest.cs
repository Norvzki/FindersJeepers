public record AssignDriversRequest
{
    public int JeepId { get; set; }
    public List<int> DriverIds { get; set; }

}
