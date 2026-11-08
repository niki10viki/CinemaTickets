namespace CinemaTickets.Legacy;

public sealed class DiscountPolicy
{
    public decimal For(decimal subtotal, ViewerKind kind, int doneCount)
    {
        var rate = RateFor(kind, doneCount);
        return Math.Min(subtotal * rate,
            PricingRules.MaxDiscount);
    }

    private static decimal RateFor(ViewerKind kind, int doneCount)
    {
        return kind switch
        {
            ViewerKind.Vip => PricingRules.VipRate,
            ViewerKind.Staff => PricingRules.StaffRate,
            _ => doneCount > PricingRules.LoyalOrders ? PricingRules.LoyalRate : 0m
        };
    }
}
