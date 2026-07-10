namespace Persistence.Repositories;

using Base.Persistence;
using Core.Contracts;
using Core.Entities;

public class TrafficHotspotRepository : GenericRepository<TrafficHotspot>, ITrafficHotspotRepository
{
    public TrafficHotspotRepository(ApplicationDbContext context) : base(context) { }
}
