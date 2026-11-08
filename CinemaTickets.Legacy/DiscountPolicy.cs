namespace CinemaTickets.Legacy;

public sealed class DiscountPolicy
{
    public decimal For(decimal subtotal, Viewer viewer)
    {
        var rate = RateFor(viewer);
        return Math.Min(subtotal * rate,
            PricingRules.MaxDiscount);
    }

    private static decimal RateFor(Viewer viewer)
    {
        return viewer.Kind switch
        {
            ViewerKind.Vip => PricingRules.VipRate,
            ViewerKind.Staff => PricingRules.StaffRate,
            _ => viewer.IsLoyal ? PricingRules.LoyalRate : 0m
        };
    }
}
