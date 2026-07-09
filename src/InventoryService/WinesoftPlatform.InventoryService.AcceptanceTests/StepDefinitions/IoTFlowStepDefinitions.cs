using Microsoft.EntityFrameworkCore;
using Moq;
using TechTalk.SpecFlow;
using WinesoftPlatform.API.Inventory.Application.Internal.CommandServices;
using WinesoftPlatform.API.Inventory.Application.Internal.QueryServices;
using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Inventory.Domain.Model.Commands;
using WinesoftPlatform.API.Inventory.Domain.Model.Queries;
using WinesoftPlatform.API.Inventory.Domain.Services;
using WinesoftPlatform.API.Inventory.Infrastructure.Persistence.Repositories;
using WinesoftPlatform.API.Shared.Domain.Repositories;
using WinesoftPlatform.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using WinesoftPlatform.InventoryService.Infrastructure.Persistence.EFC.Configuration;

namespace WinesoftPlatform.InventoryService.AcceptanceTests.StepDefinitions;

[Binding]
public class IoTFlowStepDefinitions
{
    private readonly InventoryDbContext _context;
    private readonly SensorAlertRepository _sensorAlertRepository;
    private readonly SupplyRepository _supplyRepository;
    private readonly UnitOfWork _unitOfWork;
    private readonly AlertEngine _alertEngine;
    private readonly Mock<MassTransit.IPublishEndpoint> _publishEndpointMock;
    private readonly InventorySubject _inventorySubject;
    private readonly SensorAlertCommandService _sensorAlertCommandService;
    private readonly SensorAlertQueryService _sensorAlertQueryService;
    private readonly SupplyCommandService _supplyCommandService;

    // State variables for steps
    private string _sensorType = string.Empty;
    private double _value;
    private string _unit = string.Empty;
    private string _deviceId = string.Empty;
    private int _ownerId;
    private DateTime _timestamp;
    private SensorAlert? _lastAlert;
    private Supply? _supply;
    private int _targetQuantity;
    private List<SensorAlert>? _alertsList;
    private int _totalAlerts;

    public IoTFlowStepDefinitions()
    {
        // Setup in-memory DbContext
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new InventoryDbContext(options);

        // Repositories
        _sensorAlertRepository = new SensorAlertRepository(_context);
        _supplyRepository = new SupplyRepository(_context);
        _unitOfWork = new UnitOfWork(_context);

        // Domain Services
        _alertEngine = new AlertEngine(_sensorAlertRepository);
        _publishEndpointMock = new Mock<MassTransit.IPublishEndpoint>();

        // InventorySubject with AlertEngine observer
        var observers = new List<IInventoryObserver> { _alertEngine };
        _inventorySubject = new InventorySubject(observers, _publishEndpointMock.Object);

        // Command and Query Services
        _sensorAlertCommandService = new SensorAlertCommandService(_sensorAlertRepository, _inventorySubject, _unitOfWork);
        _sensorAlertQueryService = new SensorAlertQueryService(_sensorAlertRepository);
        _supplyCommandService = new SupplyCommandService(_supplyRepository, _inventorySubject, _unitOfWork);
    }

    [Given(@"the IoT simulator generates a normal temperature reading of (.*) °C")]
    [Given(@"the IoT simulator generates a critical temperature reading of (.*) °C")]
    public void GivenTheIoTSimulatorGeneratesATemperatureReadingOfC(double temp)
    {
        _sensorType = "temperature";
        _value = temp;
        _unit = "°C";
        _deviceId = "TEMP-01";
        _ownerId = 1;
        _timestamp = DateTime.UtcNow;
    }

    [When(@"the telemetry reading is sent to the sensor alerts endpoint")]
    public async Task WhenTheTelemetryReadingIsSentToTheSensorAlertsEndpoint()
    {
        // Status and IsAnomaly are classified by AlertEngine in NotifySensorReadingAsync, so we can pass dummy values here
        var command = new CreateSensorAlertCommand(_deviceId, _sensorType, _value, _unit, _timestamp, "NORMAL", false, _ownerId);
        _lastAlert = await _sensorAlertCommandService.Handle(command);
    }

    [Then(@"the reading should be persisted in the system")]
    [Then(@"a critical alert should be created in the system")]
    public void ThenTheReadingShouldBePersistedInTheSystem()
    {
        Assert.NotNull(_lastAlert);
    }

    [Then(@"the status should be ""(.*)""")]
    public void ThenTheStatusShouldBe(string expectedStatus)
    {
        Assert.NotNull(_lastAlert);
        Assert.Equal(expectedStatus, _lastAlert.Status);
    }

    [Then(@"the reading should not be marked as an anomaly")]
    public void ThenTheReadingShouldNotBeMarkedAsAnAnomaly()
    {
        Assert.NotNull(_lastAlert);
        Assert.False(_lastAlert.IsAnomaly);
    }

    [Then(@"the reading should be marked as an anomaly")]
    public void ThenTheReadingShouldBeMarkedAsAnAnomaly()
    {
        Assert.NotNull(_lastAlert);
        Assert.True(_lastAlert.IsAnomaly);
    }

    [Given(@"the stock of a supply falls to (.*) units")]
    public async Task GivenTheStockOfASupplyFallsToUnits(int quantity)
    {
        _targetQuantity = quantity;
        _ownerId = 1;

        // Seed initial supply
        var command = new CreateSupplyCommand("Bottle Corks", 100, "units", "CorkCorp", 1.5m, DateTime.UtcNow, _ownerId);
        _supply = new Supply(command);

        await _supplyRepository.AddAsync(_supply);
        await _unitOfWork.CompleteAsync();
    }

    [When(@"the supply quantity is updated in the inventory")]
    public async Task WhenTheSupplyQuantityIsUpdatedInTheInventory()
    {
        Assert.NotNull(_supply);
        var command = new UpdateSupplyCommand(
            _supply.Id,
            _supply.SupplyName,
            _targetQuantity,
            _supply.Unit,
            _supply.Supplier,
            _supply.Price,
            _supply.Date,
            _supply.OwnerId
        );

        _supply = await _supplyCommandService.Handle(command);
    }

    [Then(@"a critical stock alert should be generated for the supply")]
    public async Task ThenACriticalStockAlertShouldBeGeneratedForTheSupply()
    {
        Assert.NotNull(_supply);
        var deviceId = $"SUPPLY-{_supply.Id}";
        
        // Find alert generated by observer
        var alerts = await _context.SensorAlerts.ToListAsync();
        _lastAlert = alerts.FirstOrDefault(a => a.DeviceId == deviceId && a.SensorType == "stock");

        Assert.NotNull(_lastAlert);
    }

    [Then(@"the status of the alert should be ""(.*)""")]
    public void ThenTheStatusOfTheAlertShouldBe(string expectedStatus)
    {
        Assert.NotNull(_lastAlert);
        Assert.Equal(expectedStatus, _lastAlert.Status);
    }

    [Then(@"the alert should be marked as an anomaly")]
    public void ThenTheAlertShouldBeMarkedAsAnAnomaly()
    {
        Assert.NotNull(_lastAlert);
        Assert.True(_lastAlert.IsAnomaly);
    }

    [Given(@"there are several active sensor alerts in the database")]
    public async Task GivenThereAreSeveralActiveSensorAlertsInTheDatabase()
    {
        _ownerId = 1;
        var now = DateTime.UtcNow;

        var alerts = new List<SensorAlert>
        {
            new("TEMP-01", "temperature", 22.0, "°C", now.AddMinutes(-5), "NORMAL", false, _ownerId),
            new("TEMP-02", "temperature", 35.0, "°C", now.AddMinutes(-2), "CRITICAL", true, _ownerId),
            new("HUM-01", "humidity", 45.0, "%", now.AddMinutes(-10), "NORMAL", false, _ownerId),
            new("SUPPLY-1", "stock", 5, "units", now.AddMinutes(-1), "CRITICAL", true, _ownerId),
            new("TEMP-03", "temperature", 12.0, "°C", now.AddMinutes(-7), "WARNING", true, _ownerId),
        };

        await _context.SensorAlerts.AddRangeAsync(alerts);
        await _context.SaveChangesAsync();
    }

    [When(@"the operator requests the list of all active alerts")]
    public async Task WhenTheOperatorRequestsTheListOfAllActiveAlerts()
    {
        var query = new GetAllSensorAlertsQuery(_ownerId, null, null, 1, 20);
        var (items, totalItems) = await _sensorAlertQueryService.Handle(query);
        
        _alertsList = items.ToList();
        _totalAlerts = totalItems;
    }

    [Then(@"the system should return a paginated list of alerts")]
    public void ThenTheSystemShouldReturnAPaginatedListOfAlerts()
    {
        Assert.NotNull(_alertsList);
        Assert.NotEmpty(_alertsList);
        Assert.Equal(5, _totalAlerts);
    }

    [Then(@"the alerts should be ordered by timestamp descending")]
    public void ThenTheAlertsShouldBeOrderedByTimestampDescending()
    {
        Assert.NotNull(_alertsList);
        for (int i = 0; i < _alertsList.Count - 1; i++)
        {
            Assert.True(_alertsList[i].Timestamp >= _alertsList[i + 1].Timestamp,
                $"Alert at index {i} ({_alertsList[i].Timestamp}) is not newer than alert at index {i + 1} ({_alertsList[i + 1].Timestamp})");
        }
    }
}
