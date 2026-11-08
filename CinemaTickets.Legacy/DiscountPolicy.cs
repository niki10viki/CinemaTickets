namespace CinemaTickets.Legacy;

public sealed class DiscountPolicy
{
    // порядок правил важливий: діє перше, що дало ненульову ставку
    private readonly IReadOnlyList<IDiscountRule> _rules = new IDiscountRule[]
    {
        new VipDiscountRule(),
        new StaffDiscountRule(),
        new LoyalDiscountRule()
    };

    public decimal For(decimal subtotal, Viewer viewer)
    {
        var rate = RateFor(viewer);
        return Math.Min(subtotal * rate,
            PricingRules.MaxDiscount);
    }

    private decimal RateFor(Viewer viewer)
    {
        foreach (var rule in _rules)
        {
            var rate = rule.RateFor(viewer);
            if (rate > 0m) return rate;
        }
        return 0m;
    }
}
