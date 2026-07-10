namespace Persistence.Repositories;

using Base.Persistence;
using Core.Contracts;
using Core.Entities;

public class DistrictRepository : GenericRepository<District>, IDistrictRepository
{
    public DistrictRepository(ApplicationDbContext context) : base(context) { }
}
