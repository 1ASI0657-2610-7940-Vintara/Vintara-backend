using System.Net.Mime;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WinesoftPlatform.API.Purchase.Domain.Model.Commands;
using WinesoftPlatform.API.Purchase.Domain.Model.Queries;
using WinesoftPlatform.API.Purchase.Domain.Services;
using WinesoftPlatform.API.Purchase.Interfaces.REST.Resources;
using WinesoftPlatform.API.Purchase.Interfaces.REST.Transform;

namespace WinesoftPlatform.API.Purchase.Interfaces.REST;

/// <summary>
///     Controller for managing purchase orders.
/// </summary>
/// <param name="orderCommandService">The order command service.</param>
/// <param name="orderQueryService">The order query service.</param>
[Authorize]
[ApiController]
[Route("api/v1/purchase-orders")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Purchase")]
public class PurchaseOrdersController(
    IOrderCommandService orderCommandService,
    IOrderQueryService orderQueryService)
    : ControllerBase
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

    /// <summary>
    ///     Create a new order.
    /// </summary>
    /// <param name="resource">The order creation resource.</param>
    /// <returns>The created order resource.</returns>
    [HttpPost]
    [SwaggerOperation(Summary = "Create a new order", OperationId = "CreateOrder")]
    [SwaggerResponse(StatusCodes.Status201Created, "The order was created", typeof(OrderResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "The order could not be created")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderResource resource)
    {
        try
        {
            var command = CreateOrderCommandFromResourceAssembler.ToCommandFromResource(resource, GetOwnerId());
            var order = await orderCommandService.Handle(command);

            if (order is null) return BadRequest();

            var orderResource = OrderResourceFromEntityAssembler.ToResourceFromEntity(order);
            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, orderResource);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    ///     Get order by ID.
    /// </summary>
    /// <param name="id">The order identifier.</param>
    /// <returns>The order resource.</returns>
    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get order by ID", OperationId = "GetOrderById")]
    [SwaggerResponse(StatusCodes.Status200OK, "The order was found", typeof(OrderResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The order was not found")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        try
        {
            var query = new GetOrderByIdQuery(id, GetOwnerId());
            var order = await orderQueryService.Handle(query);

            if (order is null) return NotFound();

            var resource = OrderResourceFromEntityAssembler.ToResourceFromEntity(order);
            return Ok(resource);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { error = ex.Message });
        }
    }

    /// <summary>
    ///     Get all orders.
    /// </summary>
    /// <returns>A list of order resources.</returns>
    [HttpGet]
    [SwaggerOperation(Summary = "Get all orders", OperationId = "GetAllOrders")]
    [SwaggerResponse(StatusCodes.Status200OK, "The list of orders", typeof(IEnumerable<OrderResource>))]
    public async Task<IActionResult> GetAllOrders()
    {
        try
        {
            var query = new GetAllOrdersQuery(GetOwnerId());
            var orders = await orderQueryService.Handle(query);
            var resources = orders.Select(OrderResourceFromEntityAssembler.ToResourceFromEntity);
            return Ok(resources);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { error = ex.Message });
        }
    }

    /// <summary>
    ///     Update an existing order.
    /// </summary>
    /// <param name="id">The order identifier.</param>
    /// <param name="resource">The order update resource.</param>
    /// <returns>The updated order resource.</returns>
    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "Update an existing order", OperationId = "UpdateOrder")]
    [SwaggerResponse(StatusCodes.Status200OK, "The order was updated", typeof(OrderResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The order was not found")]
    public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderResource resource)
    {
        try
        {
            var command = UpdateOrderCommandFromResourceAssembler.ToCommandFromResource(id, resource, GetOwnerId());
            var order = await orderCommandService.Handle(command);

            if (order is null) return NotFound();

            var orderResource = OrderResourceFromEntityAssembler.ToResourceFromEntity(order);
            return Ok(orderResource);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    ///     Delete an order.
    /// </summary>
    /// <param name="id">The order identifier.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "Delete an order", OperationId = "DeleteOrder")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "The order was deleted")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The order was not found")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        try
        {
            var command = new DeleteOrderCommand(id, GetOwnerId());
            var result = await orderCommandService.Handle(command);

            if (!result) return NotFound();
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}