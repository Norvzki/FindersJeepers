public class JeepneyOption
{
    public int Id { get; set; }
    public string PlateNumber {  get; set; } = string.Empty;
    public string BodyNumber {  get; set; } = string.Empty;
    public string RouteCode { get; set; } = string.Empty;
    public int NumberOfDrivers { get; set; }
    public int Capacity { get; set; }
}