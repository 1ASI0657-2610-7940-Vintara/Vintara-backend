using Moq;
using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Inventory.Domain.Repositories;
using WinesoftPlatform.API.Inventory.Domain.Services;

namespace WinesoftPlatform.InventoryService.Tests;

public class AlertEngineTests
{
    private readonly Mock<ISensorAlertRepository> _sensorAlertRepositoryMock;
    private readonly AlertEngine _alertEngine;

    public AlertEngineTests()
    {
        _sensorAlertRepositoryMock = new Mock<ISensorAlertRepository>();
        _alertEngine = new AlertEngine(_sensorAlertRepositoryMock.Object);
    }

    [Fact]
    public async Task OnSensorReadingReceivedAsync_TemperatureNormal_ReturnsNormalStatus()
    {
        // Arrange
        var deviceId = "TEMP-01";
        var sensorType = "temperature";
        var value = 20.0;
        var unit = "°C";
        var timestamp = DateTime.UtcNow;
        var ownerId = 1;

        _sensorAlertRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<SensorAlert>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _alertEngine.OnSensorReadingReceivedAsync(deviceId, sensorType, value, unit, timestamp, ownerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("NORMAL", result.Status);
        Assert.False(result.IsAnomaly);
        Assert.Equal(ownerId, result.OwnerId);
        
        _sensorAlertRepositoryMock.Verify(r => r.AddAsync(It.Is<SensorAlert>(a => 
            a.DeviceId == deviceId && 
            a.SensorType == sensorType && 
            a.Value == value && 
            a.Status == "NORMAL" && 
            !a.IsAnomaly
        )), Times.Once);
    }

    [Fact]
    public async Task OnSensorReadingReceivedAsync_TemperatureAboveThreshold_ReturnsCriticalStatus()
    {
        // Arrange
        var deviceId = "TEMP-01";
        var sensorType = "temperature";
        var value = 32.5;
        var unit = "°C";
        var timestamp = DateTime.UtcNow;
        var ownerId = 1;

        _sensorAlertRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<SensorAlert>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _alertEngine.OnSensorReadingReceivedAsync(deviceId, sensorType, value, unit, timestamp, ownerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("CRITICAL", result.Status);
        Assert.True(result.IsAnomaly);
        Assert.Equal(ownerId, result.OwnerId);

        _sensorAlertRepositoryMock.Verify(r => r.AddAsync(It.Is<SensorAlert>(a => 
            a.DeviceId == deviceId && 
            a.SensorType == sensorType && 
            a.Value == value && 
            a.Status == "CRITICAL" && 
            a.IsAnomaly
        )), Times.Once);
    }

    [Fact]
    public async Task OnSensorReadingReceivedAsync_TemperatureInWarningRange_ReturnsWarningStatus()
    {
        // Arrange
        var deviceId = "TEMP-01";
        var sensorType = "temperature";
        var value = 27.0; // Warning is < 15.0 or > 25.0, but within normal range [10, 30]
        var unit = "°C";
        var timestamp = DateTime.UtcNow;
        var ownerId = 1;

        _sensorAlertRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<SensorAlert>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _alertEngine.OnSensorReadingReceivedAsync(deviceId, sensorType, value, unit, timestamp, ownerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("WARNING", result.Status);
        Assert.True(result.IsAnomaly);
        Assert.Equal(ownerId, result.OwnerId);

        _sensorAlertRepositoryMock.Verify(r => r.AddAsync(It.Is<SensorAlert>(a => 
            a.DeviceId == deviceId && 
            a.SensorType == sensorType && 
            a.Value == value && 
            a.Status == "WARNING" && 
            a.IsAnomaly
        )), Times.Once);
    }
}
