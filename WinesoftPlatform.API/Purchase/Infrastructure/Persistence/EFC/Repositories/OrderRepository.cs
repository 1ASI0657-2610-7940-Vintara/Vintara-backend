using Microsoft.EntityFrameworkCore;
using WinesoftPlatform.API.Purchase.Domain.Model.Aggregates;
using WinesoftPlatform.API.Purchase.Domain.Repositories;
using WinesoftPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using WinesoftPlatform.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace WinesoftPlatform.API.Purchase.Infrastructure.Persistence.EFC.Repositories;

public class OrderRepository : BaseRepository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Order>> FindByCreatedDateAsync(DateTime date)
    {
        return await Context.Set<Order>()
            .Where(o => o.CreatedDate.HasValue && o.CreatedDate.Value.Date == date.Date)
            .ToListAsync();
    }
}