using System.Net.Mime;
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
[ApiController]
[Route("api/v1/purchase-orders")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Purchase")]
public class PurchaseOrdersController(
    IOrderCommandService orderCommandService,
    IOrderQueryService orderQueryService)
    : ControllerBase
{
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
        var command = CreateOrderCommandFromResourceAssembler.ToCommandFromResource(resource);
        var order = await orderCommandService.Handle(command);

        if (order is null) return BadRequest();

        var orderResource = OrderResourceFromEntityAssembler.ToResourceFromEntity(order);
        return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, orderResource);
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
        var query = new GetOrderByIdQuery(id);
        var order = await orderQueryService.Handle(query);

        if (order is null) return NotFound();

        var resource = OrderResourceFromEntityAssembler.ToResourceFromEntity(order);
        return Ok(resource);
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
        var query = new GetAllOrdersQuery();
        var orders = await orderQueryService.Handle(query);
        var resources = orders.Select(OrderResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
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
        var command = UpdateOrderCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var order = await orderCommandService.Handle(command);

        if (order is null) return NotFound();

        var orderResource = OrderResourceFromEntityAssembler.ToResourceFromEntity(order);
        return Ok(orderResource);
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
        var command = new DeleteOrderCommand(id);
        var result = await orderCommandService.Handle(command);

        if (!result) return NotFound();
        return NoContent();
    }
}