using System.Net.Mime;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WinesoftPlatform.API.Inventory.Domain.Model.Commands;
using WinesoftPlatform.API.Inventory.Domain.Model.Queries;
using WinesoftPlatform.API.Inventory.Domain.Services;
using WinesoftPlatform.API.Inventory.Interfaces.REST.Resources;
using WinesoftPlatform.API.Inventory.Interfaces.REST.Transform;

namespace WinesoftPlatform.API.Inventory.Interfaces.REST;

[ApiController]
[Route("api/v1/inventory/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Supplies")]
public class SuppliesController(
    ISupplyCommandService supplyCommandService,
    ISupplyQueryService supplyQueryService
) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create a new supply",
        Description = "Creates a new supply record in the system.",
        OperationId = "CreateSupply")]
    [SwaggerResponse(201, "Supply created successfully", typeof(SupplyResource))]
    [SwaggerResponse(400, "Invalid request", typeof(BadRequestResult))]
    [SwaggerResponse(409, "Supply already exists for this supplier", typeof(ConflictResult))]
    public async Task<IActionResult> CreateSupply([FromBody] CreateSupplyResource resource)
    {
        try
        {
            var command = CreateSupplyCommandFromResourceAssembler.ToCommandFromResource(resource);
            var result = await supplyCommandService.Handle(command);
            if (result is null) return BadRequest();

            var supplyResource = SupplyResourceFromEntityAssembler.ToResourceFromEntity(result);
            return CreatedAtAction(nameof(GetSupplyById), new { id = result.Id }, supplyResource);
        }

        catch (Exception ex)
        {
            if (ex.Message.Contains("Supply already exists", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new
                {
                    error = "A supply with this name and supplier already exists. Please edit the existing record."
                });
            }

            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all supplies",
        Description = "Retrieves the list of all supplies available in the inventory.",
        OperationId = "GetAllSupplies")]
    [SwaggerResponse(200, "List of supplies retrieved successfully", typeof(IEnumerable<SupplyResource>))]
    public async Task<IActionResult> GetAllSupplies()
    {
        var query = new GetAllSuppliesQuery();
        var result = await supplyQueryService.Handle(query);
        var resources = result.Select(SupplyResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(
        Summary = "Get supply by ID",
        Description = "Retrieves a specific supply by its unique identifier.",
        OperationId = "GetSupplyById")]
    [SwaggerResponse(200, "Supply retrieved successfully", typeof(SupplyResource))]
    [SwaggerResponse(404, "Supply not found")]
    public async Task<IActionResult> GetSupplyById([FromRoute] int id)
    {
        var query = new GetSupplyByIdQuery(id);
        var result = await supplyQueryService.Handle(query);
        if (result is null) return NotFound();
        var resource = SupplyResourceFromEntityAssembler.ToResourceFromEntity(result);
        return Ok(resource);
    }

    [HttpPut("{id:int}")]
    [SwaggerOperation(
        Summary = "Update a supply",
        Description = "Updates an existing supply based on its ID.",
        OperationId = "UpdateSupply")]
    [SwaggerResponse(200, "Supply updated successfully", typeof(SupplyResource))]
    [SwaggerResponse(404, "Supply not found")]
    public async Task<IActionResult> UpdateSupply([FromRoute] int id, [FromBody] UpdateSupplyResource resource)
    {
        var command = UpdateSupplyCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await supplyCommandService.Handle(command);
        if (result is null) return NotFound();

        var supplyResource = SupplyResourceFromEntityAssembler.ToResourceFromEntity(result);
        return Ok(supplyResource);
    }

    [HttpDelete("{id:int}")]
    [SwaggerOperation(
        Summary = "Delete a supply",
        Description = "Deletes an existing supply from the system.",
        OperationId = "DeleteSupply")]
    [SwaggerResponse(204, "Supply deleted successfully")]
    [SwaggerResponse(404, "Supply not found")]
    public async Task<IActionResult> DeleteSupply([FromRoute] int id)
    {
        var deleteCommand = new DeleteSupplyCommand(id);
        var result = await supplyCommandService.Handle(deleteCommand);

        if (!result) return NotFound();
        return NoContent();
    }
}