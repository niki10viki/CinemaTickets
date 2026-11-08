namespace CinemaTickets.Legacy;

public sealed class ShippingPolicy
{
    public decimal For(decimal payable) =>
        payable < PricingRules.FreeShipFrom
            ? PricingRules.ShipCost : 0m;
}
