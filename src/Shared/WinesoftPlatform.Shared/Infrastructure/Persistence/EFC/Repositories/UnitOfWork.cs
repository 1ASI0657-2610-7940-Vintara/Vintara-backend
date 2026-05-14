using Microsoft.EntityFrameworkCore;
using WinesoftPlatform.API.Shared.Domain.Repositories;

namespace WinesoftPlatform.API.Shared.Infrastructure.Persistence.EFC.Repositories;

public class UnitOfWork(DbContext context) : IUnitOfWork
{
    public async Task CompleteAsync()
    {
        await context.SaveChangesAsync();
    }
}