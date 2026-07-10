namespace Persistence.Repositories;

using Base.Persistence;
using Core.Contracts;
using Core.Entities;

public class TrafficRepository : GenericRepository<Traffic>, ITrafficRepository
{
    public TrafficRepository(ApplicationDbContext context) : base(context) { }
}
