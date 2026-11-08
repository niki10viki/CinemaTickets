namespace CinemaTickets.Legacy;

public interface IDiscountRule
{
    // ставка знижки або 0m, якщо правило до глядача не застосовне
    decimal RateFor(Viewer viewer);
}

public sealed class VipDiscountRule : IDiscountRule
{
    public decimal RateFor(Viewer viewer) =>
        viewer.Kind == ViewerKind.Vip ? PricingRules.VipRate : 0m;
}

public sealed class StaffDiscountRule : IDiscountRule
{
    public decimal RateFor(Viewer viewer) =>
        viewer.Kind == ViewerKind.Staff ? PricingRules.StaffRate : 0m;
}

public sealed class LoyalDiscountRule : IDiscountRule
{
    public decimal RateFor(Viewer viewer) =>
        viewer.IsLoyal ? PricingRules.LoyalRate : 0m;
}
