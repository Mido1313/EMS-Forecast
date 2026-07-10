namespace Persistence.Repositories;

using Base.Persistence;
using Core.Contracts;
using Core.Entities;

public class AccidentHistoryRepository : GenericRepository<AccidentHistory>, IAccidentHistoryRepository
{
    public AccidentHistoryRepository(ApplicationDbContext context) : base(context) { }
}
