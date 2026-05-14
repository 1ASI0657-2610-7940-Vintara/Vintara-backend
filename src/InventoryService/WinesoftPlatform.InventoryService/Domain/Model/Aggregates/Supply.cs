using WinesoftPlatform.API.Inventory.Domain.Model.Commands;

namespace WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;

public partial class Supply
{
    public int Id { get; }
    public string SupplyName { get; private set; }
    public int Quantity { get; private set; }
    public string Unit { get; private set; }
    public string Supplier { get; private set; }
    public decimal Price { get; private set; }
    public DateTime Date { get; private set; }

    protected Supply()
    {
        SupplyName = string.Empty;
        Quantity = int.MinValue;
        Unit = string.Empty;
        Supplier = string.Empty;
        Price = decimal.MinValue;
        Date = DateTime.MinValue;
    }
    
    public Supply(CreateSupplyCommand command)
    {
        SupplyName = command.SupplyName;
        Quantity = command.Quantity;
        Unit = command.Unit;
        Supplier = command.Supplier;
        Price = command.Price;
        Date = command.Date;
    }
    
    public void UpdateDetails(string supplyName, int quantity, string unit, string supplier, decimal price, DateTime date)
    {
        SupplyName = supplyName;
        Quantity = quantity;
        Unit = unit;
        Supplier = supplier;
        Price = price;
        Date = date;
    }
}