namespace Persistence.Repositories;

using Base.Persistence;
using Core.Contracts;
using Core.Entities;

public class NursingHomeRepository : GenericRepository<NursingHome>, INursingHomeRepository
{
    public NursingHomeRepository(ApplicationDbContext context) : base(context) { }
}
