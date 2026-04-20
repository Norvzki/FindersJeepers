
using Microsoft.EntityFrameworkCore;

public class DriverRepository : Repository<Driver>, IDriverRepository
{
    public DriverRepository(MyDbContext context) : base(context)
    {
    }
}

public interface IDriverRepository : IRepository<Driver>
{

}