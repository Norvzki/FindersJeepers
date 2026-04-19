public class Driver : AggregateRoot
{
    public int Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string LicenseNumber { get; private set; }
    public string ContactNumber { get; private set; }
    public DateTime DateHired {  get; private set; }
    private Driver()
    {
         
    }
    public static Driver Create(string fName, string lName, string licenseNumber, string contactNumber, DateTime dateHired) 
    {
        if (string.IsNullOrWhiteSpace(fName)) throw new DomainException("First Name cannot be empty!");
        if (string.IsNullOrWhiteSpace(lName)) throw new DomainException("Last Name cannot be empty!");
        if (string.IsNullOrWhiteSpace(licenseNumber)) throw new DomainException("A driver must have a license number!");
        if (!IsValidContactNumber(contactNumber)) throw new DomainException("Invalid contact number!");
        if (dateHired == DateTime.MinValue) throw new DomainException("Please check the date hired."); 

        return new Driver
        {
            FirstName = fName,
            LastName = lName,
            LicenseNumber = licenseNumber,
            ContactNumber = contactNumber,
            DateHired = dateHired
        };
    }

    private static bool IsValidContactNumber(string number)
    {
        // use regex to validate number here
        return true;
    } 
}
