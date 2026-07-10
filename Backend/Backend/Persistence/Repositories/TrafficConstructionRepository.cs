namespace Persistence.Repositories;

using Base.Persistence;
using Core.Contracts;
using Core.Entities;

public class TrafficConstructionRepository : GenericRepository<TrafficConstruction>, ITrafficConstructionRepository
{
    public TrafficConstructionRepository(ApplicationDbContext context) : base(context) { }
}
