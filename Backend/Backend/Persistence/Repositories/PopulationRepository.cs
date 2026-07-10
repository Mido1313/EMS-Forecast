namespace Persistence.Repositories;

using Base.Persistence;
using Core.Contracts;
using Core.Entities;

public class PopulationRepository : GenericRepository<Population>, IPopulationRepository
{
    public PopulationRepository(ApplicationDbContext context) : base(context) { }
}
