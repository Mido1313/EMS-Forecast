namespace Persistence.Repositories;

using Base.Persistence;
using Core.Contracts;
using Core.Entities;

public class IncidentTypeRepository : GenericRepository<IncidentType>, IIncidentTypeRepository
{
    public IncidentTypeRepository(ApplicationDbContext context) : base(context) { }
}
