using System.Net.Mime;
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
[Tags("Sensor Alerts")]
public class SensorAlertsController(
    ISensorAlertCommandService sensorAlertCommandService,
    ISensorAlertQueryService sensorAlertQueryService
) : ControllerBase
{
    /// <summary>
    /// Receives telemetry data from the IoT Simulator and persists it as a sensor alert.
    /// </summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Ingest IoT telemetry reading",
        Description = "Receives a sensor reading from the IoT Simulator Service and creates a sensor alert record.",
        OperationId = "CreateSensorAlert")]
    [SwaggerResponse(201, "Sensor alert created successfully", typeof(SensorAlertResource))]
    [SwaggerResponse(400, "Invalid request")]
    public async Task<IActionResult> CreateSensorAlert([FromBody] CreateSensorAlertResource resource)
    {
        var command = CreateSensorAlertCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await sensorAlertCommandService.Handle(command);
        if (result is null) return BadRequest();

        var alertResource = SensorAlertResourceFromEntityAssembler.ToResourceFromEntity(result);
        return CreatedAtAction(nameof(GetSensorAlertById), new { id = result.Id }, alertResource);
    }

    /// <summary>
    /// Retrieves paginated sensor alerts with optional filtering by status and sensor type.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "List sensor alerts",
        Description = "Retrieves a paginated list of sensor alerts, optionally filtered by status and sensor type.",
        OperationId = "GetAllSensorAlerts")]
    [SwaggerResponse(200, "Paginated list of sensor alerts")]
    public async Task<IActionResult> GetAllSensorAlerts(
        [FromQuery] string? status,
        [FromQuery] string? sensorType,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
    {
        var query = new GetAllSensorAlertsQuery(status, sensorType, page, size);
        var (items, totalItems) = await sensorAlertQueryService.Handle(query);

        var resources = items.Select(SensorAlertResourceFromEntityAssembler.ToResourceFromEntity);

        return Ok(new
        {
            items = resources,
            page,
            size,
            totalItems,
            totalPages = (int)Math.Ceiling((double)totalItems / size)
        });
    }

    /// <summary>
    /// Retrieves a single sensor alert by its ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [SwaggerOperation(
        Summary = "Get sensor alert by ID",
        Description = "Retrieves a specific sensor alert by its unique identifier.",
        OperationId = "GetSensorAlertById")]
    [SwaggerResponse(200, "Sensor alert retrieved successfully", typeof(SensorAlertResource))]
    [SwaggerResponse(404, "Sensor alert not found")]
    public async Task<IActionResult> GetSensorAlertById([FromRoute] int id)
    {
        var query = new GetSensorAlertByIdQuery(id);
        var result = await sensorAlertQueryService.Handle(query);
        if (result is null) return NotFound();

        var resource = SensorAlertResourceFromEntityAssembler.ToResourceFromEntity(result);
        return Ok(resource);
    }

    /// <summary>
    /// Marks a sensor alert as acknowledged by an operator.
    /// </summary>
    [HttpPatch("{id:int}/acknowledge")]
    [SwaggerOperation(
        Summary = "Acknowledge a sensor alert",
        Description = "Marks a sensor alert as acknowledged/resolved by an operator.",
        OperationId = "AcknowledgeSensorAlert")]
    [SwaggerResponse(200, "Sensor alert acknowledged successfully")]
    [SwaggerResponse(404, "Sensor alert not found")]
    public async Task<IActionResult> AcknowledgeSensorAlert([FromRoute] int id)
    {
        var command = new AcknowledgeSensorAlertCommand(id);
        var result = await sensorAlertCommandService.Handle(command);
        if (result is null) return NotFound();

        return Ok(new
        {
            id = result.Id,
            acknowledged = result.Acknowledged,
            acknowledgedAt = result.AcknowledgedAt
        });
    }
}
