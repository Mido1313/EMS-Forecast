namespace Persistence.Repositories;

using Base.Persistence;
using Core.Contracts;
using Core.Entities;

public class TrafficAccidentRepository : GenericRepository<TrafficAccident>, ITrafficAccidentRepository
{
    public TrafficAccidentRepository(ApplicationDbContext context) : base(context) { }
}
