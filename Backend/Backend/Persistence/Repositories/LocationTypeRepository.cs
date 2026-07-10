namespace Persistence.Repositories;

using Base.Persistence;
using Core.Contracts;
using Core.Entities;

public class LocationTypeRepository : GenericRepository<LocationType>, ILocationTypeRepository
{
    public LocationTypeRepository(ApplicationDbContext context) : base(context) { }
}
