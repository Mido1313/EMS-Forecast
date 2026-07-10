namespace Persistence.Repositories;

using Base.Persistence;
using Core.Contracts;
using Core.Entities;

public class PublicHolidayRepository : GenericRepository<PublicHoliday>, IPublicHolidayRepository
{
    public PublicHolidayRepository(ApplicationDbContext context) : base(context) { }
}
