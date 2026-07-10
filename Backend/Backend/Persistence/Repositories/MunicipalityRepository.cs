namespace Persistence.Repositories;

using Base.Persistence;
using Core.Contracts;
using Core.Entities;

public class MunicipalityRepository : GenericRepository<Municipality>, IMunicipalityRepository
{
    public MunicipalityRepository(ApplicationDbContext context) : base(context) { }
}
