using Microsoft.EntityFrameworkCore;
using WinesoftPlatform.API.Inventory.Application.Internal.QueryServices;
using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Inventory.Domain.Model.Commands;
using WinesoftPlatform.API.Inventory.Domain.Model.Queries;
using WinesoftPlatform.API.Inventory.Infrastructure.Persistence.Repositories;
using WinesoftPlatform.InventoryService.Infrastructure.Persistence.EFC.Configuration;

namespace WinesoftPlatform.InventoryService.Tests;

public class SupplyQueryServiceTests
{
    private DbContextOptions<InventoryDbContext> CreateNewContextOptions()
    {
        return new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task Handle_GetAllSuppliesQueryForSpecificOwner_ReturnsOnlySuppliesBelongingToThatOwner()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using (var context = new InventoryDbContext(options))
        {
            var supply1 = new Supply(new CreateSupplyCommand("Pinot Noir Grapes", 100, "kg", "Vistalba", 15.5m, DateTime.UtcNow, 1));
            var supply2 = new Supply(new CreateSupplyCommand("Cabernet Grapes", 200, "kg", "Catena", 18.0m, DateTime.UtcNow, 2));

            await context.Supplies.AddRangeAsync(supply1, supply2);
            await context.SaveChangesAsync();
        }

        using (var context = new InventoryDbContext(options))
        {
            var repository = new SupplyRepository(context);
            var queryService = new SupplyQueryService(repository);
            var query = new GetAllSuppliesQuery(1); // JWT OwnerId = 1

            // Act
            var result = await queryService.Handle(query);

            // Assert
            var suppliesList = result.ToList();
            Assert.Single(suppliesList);
            Assert.Equal(1, suppliesList[0].OwnerId);
            Assert.Equal("Pinot Noir Grapes", suppliesList[0].SupplyName);
        }
    }

    [Fact]
    public async Task Handle_GetSupplyByIdQueryWithDifferentOwnerId_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var options = CreateNewContextOptions();
        int seededSupplyId;
        using (var context = new InventoryDbContext(options))
        {
            // Supply belongs to OwnerId = 2
            var supply = new Supply(new CreateSupplyCommand("Merlot Grapes", 150, "kg", "Norton", 12.0m, DateTime.UtcNow, 2));
            await context.Supplies.AddAsync(supply);
            await context.SaveChangesAsync();
            seededSupplyId = supply.Id;
        }

        using (var context = new InventoryDbContext(options))
        {
            var repository = new SupplyRepository(context);
            var queryService = new SupplyQueryService(repository);
            
            // Query with OwnerId = 1 (JWT has OwnerId=1, but supply belongs to OwnerId=2)
            var query = new GetSupplyByIdQuery(seededSupplyId, 1);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => queryService.Handle(query));
        }
    }

    [Fact]
    public async Task Handle_GetSupplyByIdQueryWithMatchingOwnerId_ReturnsSupplySuccessfully()
    {
        // Arrange
        var options = CreateNewContextOptions();
        int seededSupplyId;
        using (var context = new InventoryDbContext(options))
        {
            // Supply belongs to OwnerId = 1
            var supply = new Supply(new CreateSupplyCommand("Chardonnay Grapes", 80, "kg", "Salentein", 20.0m, DateTime.UtcNow, 1));
            await context.Supplies.AddAsync(supply);
            await context.SaveChangesAsync();
            seededSupplyId = supply.Id;
        }

        using (var context = new InventoryDbContext(options))
        {
            var repository = new SupplyRepository(context);
            var queryService = new SupplyQueryService(repository);
            
            // Query with OwnerId = 1 (Matches supply OwnerId)
            var query = new GetSupplyByIdQuery(seededSupplyId, 1);

            // Act
            var result = await queryService.Handle(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(seededSupplyId, result.Id);
            Assert.Equal(1, result.OwnerId);
            Assert.Equal("Chardonnay Grapes", result.SupplyName);
        }
    }

    [Fact]
    public async Task Handle_GetSupplyByIdQueryForNonExistentSupply_ReturnsNull()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using (var context = new InventoryDbContext(options))
        {
            var repository = new SupplyRepository(context);
            var queryService = new SupplyQueryService(repository);
            
            // Query for non-existent ID 999 with OwnerId = 1
            var query = new GetSupplyByIdQuery(999, 1);

            // Act
            var result = await queryService.Handle(query);

            // Assert
            Assert.Null(result);
        }
    }
}
