using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WinesoftPlatform.API.Inventory.Domain.Model.Queries;
using WinesoftPlatform.API.Inventory.Domain.Services;
using WinesoftPlatform.API.Inventory.Interfaces.REST.Resources;
using WinesoftPlatform.API.Inventory.Interfaces.REST.Transform;

namespace WinesoftPlatform.API.Inventory.Interfaces.REST;

[Authorize(Policy = "ServiceOnly")]
[ApiController]
[Route("api/internal/supplies")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Internal Supplies")]
public class InternalSuppliesController(
    ISupplyQueryService supplyQueryService
) : ControllerBase
{
    [HttpGet("all")]
    [SwaggerOperation(
        Summary = "Get all supplies internally (Service-to-Service)",
        Description = "Retrieves all supplies from all owners. Accessible only by services.",
        OperationId = "GetAllSuppliesInternal")]
    [SwaggerResponse(200, "All supplies retrieved successfully", typeof(IEnumerable<SupplyResource>))]
    public async Task<IActionResult> GetAllSuppliesInternal()
    {
        var query = new GetAllInternalSuppliesQuery();
        var result = await supplyQueryService.Handle(query);
        var resources = result.Select(SupplyResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }
}
