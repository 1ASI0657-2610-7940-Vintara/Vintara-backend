using System.Net.Mime;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WinesoftPlatform.API.Inventory.Domain.Services;
using WinesoftPlatform.API.Inventory.Domain.Model.Queries;
using WinesoftPlatform.API.Inventory.Interfaces.REST.Resources;
using WinesoftPlatform.API.Inventory.Interfaces.REST.Transform;

namespace WinesoftPlatform.API.Inventory.Interfaces.REST;

[Authorize]
[ApiController]
[Route("api/v1/inventory/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("StockMovements")]
public class StockMovementsController(
    IStockMovementCommandService stockMovementCommandService,
    IStockMovementQueryService stockMovementQueryService
) : ControllerBase
{
    private int GetOwnerId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !int.TryParse(claim.Value, out var ownerId))
        {
            throw new UnauthorizedAccessException("Owner ID is missing or invalid in JWT token.");
        }
        return ownerId;
    }

    [HttpPost]
    [SwaggerOperation(
        Summary = "Create a new stock movement",
        Description = "Creates a new stock movement record and automatically adjusts the stock level of the associated supply.",
        OperationId = "CreateStockMovement")]
    [SwaggerResponse(201, "Stock movement created successfully", typeof(StockMovementResource))]
    [SwaggerResponse(400, "Invalid request or insufficient stock")]
    public async Task<IActionResult> CreateStockMovement([FromBody] CreateStockMovementResource resource)
    {
        try
        {
            var command = CreateStockMovementCommandFromResourceAssembler.ToCommandFromResource(resource, GetOwnerId());
            var result = await stockMovementCommandService.Handle(command);
            if (result is null) return BadRequest(new { error = "Failed to process stock movement." });

            var movementResource = StockMovementResourceFromEntityAssembler.ToResourceFromEntity(result);
            return CreatedAtAction(nameof(GetAllStockMovements), movementResource);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all stock movements",
        Description = "Retrieves the list of all stock movements for the authenticated owner.",
        OperationId = "GetAllStockMovements")]
    [SwaggerResponse(200, "List of stock movements retrieved successfully", typeof(IEnumerable<StockMovementResource>))]
    public async Task<IActionResult> GetAllStockMovements()
    {
        try
        {
            var query = new GetAllStockMovementsQuery(GetOwnerId());
            var result = await stockMovementQueryService.Handle(query);
            var resources = result.Select(StockMovementResourceFromEntityAssembler.ToResourceFromEntity);
            return Ok(resources);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { error = ex.Message });
        }
    }
}
