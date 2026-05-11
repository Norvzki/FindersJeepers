public class JeepneyDriverDto
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? UnassignedAt { get; set; }

    public bool IsAvailable { get; set; }
    public string? CurrentJeepneyPlateDriving { get; set; }

}
