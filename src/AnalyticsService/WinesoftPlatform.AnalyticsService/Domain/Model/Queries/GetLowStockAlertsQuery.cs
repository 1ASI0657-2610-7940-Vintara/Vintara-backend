namespace WinesoftPlatform.API.Analytics.Domain.Model.Queries;

public record GetLowStockAlertsQuery(int OwnerId, int Threshold = 30);