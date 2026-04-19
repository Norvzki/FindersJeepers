public class Location : AggregateRoot
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }

    private Location()
    {
         
    }

    public static Location Create(string name, string description)
    {
        if (string.IsNullOrEmpty(name)) throw new DomainException("Name cannot be empty!");

        return new Location
        {
            Name = name,
            Description = description
        };
    }
}
