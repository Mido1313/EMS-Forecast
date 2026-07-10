namespace Persistence.Repositories;

using Base.Persistence;
using Core.Contracts;
using Core.Entities;

public class PostalCodeRepository : GenericRepository<PostalCode>, IPostalCodeRepository
{
    public PostalCodeRepository(ApplicationDbContext context) : base(context) { }
}
