namespace Persistence.Repositories;

using Base.Persistence;
using Core.Contracts;
using Core.Entities;

public class ResultRepository : GenericRepository<Result>, IResultRepository
{
    public ResultRepository(ApplicationDbContext context) : base(context) { }
}
