public record UpdateRouteRequest
{
    public int Id { get; set; }
    public string RouteCode { get; set; } = string.Empty;
    public int LocationStartId { get; set; }
    public int LocationEndId { get; set; }
}
