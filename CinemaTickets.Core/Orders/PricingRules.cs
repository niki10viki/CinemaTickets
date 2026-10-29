namespace CinemaTickets.Core.Orders;

/// <summary>
/// Правила ціноутворення замовлення квитків: пороги, ставки знижок і промокоди.
/// </summary>
public static class PricingRules
{
    /// <summary>Кількість квитків у рядку, з якої діє групова знижка.</summary>
    public const int BulkQuantity = 10;

    /// <summary>Частка групової знижки на рядок.</summary>
    public const decimal BulkRate = 0.05m;

    /// <summary>Частка знижки постійному глядачеві.</summary>
    public const decimal RegularViewerRate = 0.07m;

    /// <summary>Сума, понад яку діє знижка на велике замовлення, грн.</summary>
    public const decimal LargeOrderFrom = 5000m;

    /// <summary>Частка знижки на велике замовлення.</summary>
    public const decimal LargeOrderRate = 0.10m;

    /// <summary>Сума, понад яку діє знижка на середнє замовлення, грн.</summary>
    public const decimal MediumOrderFrom = 2000m;

    /// <summary>Частка знижки на середнє замовлення.</summary>
    public const decimal MediumOrderRate = 0.05m;

    /// <summary>Частка знижки на замовлення, створене в неділю.</summary>
    public const decimal SundayRate = 0.02m;

    /// <summary>Частка знижки за промокодом <see cref="SaleCoupon"/>.</summary>
    public const decimal CouponSaleRate = 0.10m;

    /// <summary>Фіксована знижка за промокодом <see cref="FixedCoupon"/>, грн.</summary>
    public const decimal CouponFixed = 200m;

    /// <summary>Сума, понад яку діє промокод <see cref="FixedCoupon"/>, грн.</summary>
    public const decimal CouponFixedFrom = 1000m;

    /// <summary>Сума, від якої сервісний збір не стягується, грн.</summary>
    public const decimal FreeServiceFeeFrom = 1000m;

    /// <summary>Промокод на знижку у відсотках.</summary>
    public const string SaleCoupon = "SALE10";

    /// <summary>Промокод на фіксовану знижку.</summary>
    public const string FixedCoupon = "MINUS200";
}
