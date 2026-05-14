using WinesoftPlatform.API.Analytics.Domain.Model.ValueObjects;
using WinesoftPlatform.API.Analytics.Interfaces.REST.Resources;

namespace WinesoftPlatform.API.Analytics.Interfaces.REST.Transform;

public static class CostsSummaryResourceFromEntityAssembler
{
    public static CostsSummaryResource ToResourceFromEntity(CostsSummary entity)
    {
        return new CostsSummaryResource(entity.TotalCost, entity.StartDate, entity.EndDate);
    }
}