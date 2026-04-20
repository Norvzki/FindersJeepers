
using Microsoft.EntityFrameworkCore;

public class JeepneyRepository : Repository<Jeepney>, IJeepneyRepository
{
    public JeepneyRepository(MyDbContext context) : base(context)
    {
    }
}

public interface IJeepneyRepository : IRepository<Jeepney>
{

}