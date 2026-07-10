namespace Persistence.Repositories;

using Base.Persistence;
using Core.Contracts;
using Core.Entities;

public class AttractionRepository : GenericRepository<Attraction>, IAttractionRepository
{
    public AttractionRepository(ApplicationDbContext context) : base(context) { }
}
