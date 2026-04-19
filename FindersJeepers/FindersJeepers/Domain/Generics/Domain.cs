
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

public class IdValidator
{
    public static bool ValidateId(int id)
    {
        if(id < 1) return false;
        return true;
    }
}

public enum TripLogType {
    Departure = 0,
    Arrival = 1,
}
public enum TripStatus
{
    Waiting = 0,
    Completed = 1,
    OnGoing = 2,
    Unavailable = 3,
}