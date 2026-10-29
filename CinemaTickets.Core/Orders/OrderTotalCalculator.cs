using CinemaTickets.Core.Domain;

namespace CinemaTickets.Core.Orders;

/// <summary>
/// Розрахунок суми замовлення квитків, розкладений на кроки з однією відповідальністю.
/// </summary>
public static class OrderTotalCalculator
{
    /// <summary>
    /// Рахує суму замовлення та наступний стан; нічого не змінює й не друкує.
    /// </summary>
    /// <param name="request">Запит на розрахунок замовлення.</param>
    /// <returns>Сума до сплати, заокруглена до двох знаків, і новий стан замовлення.</returns>
    /// <exception cref="ArgumentException">Запит порушує передумови розрахунку.</exception>
    public static TotalResult Calculate(TotalRequest request)
    {
        OrderGuards.EnsureRequestValid(request);
        decimal total = SubtotalOf(request.Lines);
        total -= ViewerDiscountOf(total, request.Viewer.IsRegular);
        total -= VolumeDiscountOf(total);
        total -= CouponDiscountOf(total, request.CouponCode);
        total -= SundayDiscountOf(total, request.CreatedAt);
        total += ServiceFeeOf(total, request.ServiceFee);
        total = request.Status == TicketStatus.Cancelled
            ? 0m
            : Math.Round(Math.Max(total, 0m), 2);
        return new TotalResult(total, NextStatusOf(request.Status, total));
    }

    /// <summary>
    /// Рахує суму рядків замовлення з груповою знижкою на великі рядки.
    /// </summary>
    /// <param name="lines">Рядки замовлення.</param>
    /// <returns>Сума рядків до знижок на все замовлення.</returns>
    public static decimal SubtotalOf(IReadOnlyList<SeatLine> lines)
    {
        OrderGuards.EnsureLinesValid(lines);
        decimal subtotal = 0m;
        foreach (SeatLine line in lines)
        {
            decimal sum = line.Quantity * line.UnitPrice;
            if (line.Quantity >= PricingRules.BulkQuantity)
            {
                sum -= sum * PricingRules.BulkRate;
            }

            subtotal += sum;
        }

        return subtotal;
    }

    /// <summary>
    /// Рахує знижку постійному глядачеві.
    /// </summary>
    /// <param name="amount">Сума, від якої рахується знижка.</param>
    /// <param name="isRegularViewer">Ознака постійного глядача.</param>
    /// <returns>Розмір знижки, грн.</returns>
    public static decimal ViewerDiscountOf(decimal amount, bool isRegularViewer)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return isRegularViewer ? amount * PricingRules.RegularViewerRate : 0m;
    }

    /// <summary>
    /// Рахує знижку за обсягом замовлення.
    /// </summary>
    /// <param name="amount">Сума, від якої рахується знижка.</param>
    /// <returns>Розмір знижки, грн.</returns>
    public static decimal VolumeDiscountOf(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        if (amount > PricingRules.LargeOrderFrom)
        {
            return amount * PricingRules.LargeOrderRate;
        }

        if (amount > PricingRules.MediumOrderFrom)
        {
            return amount * PricingRules.MediumOrderRate;
        }

        return 0m;
    }

    /// <summary>
    /// Рахує знижку за промокодом.
    /// </summary>
    /// <param name="amount">Сума, від якої рахується знижка.</param>
    /// <param name="couponCode">Промокод; null або невідомий код знижки не дає.</param>
    /// <returns>Розмір знижки, грн.</returns>
    public static decimal CouponDiscountOf(decimal amount, string? couponCode)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return couponCode switch
        {
            PricingRules.SaleCoupon => amount * PricingRules.CouponSaleRate,
            PricingRules.FixedCoupon when amount > PricingRules.CouponFixedFrom
                => PricingRules.CouponFixed,
            _ => 0m,
        };
    }

    /// <summary>
    /// Рахує знижку на замовлення, створене в неділю.
    /// </summary>
    /// <param name="amount">Сума, від якої рахується знижка.</param>
    /// <param name="createdAt">Дата створення замовлення.</param>
    /// <returns>Розмір знижки, грн.</returns>
    public static decimal SundayDiscountOf(decimal amount, DateOnly createdAt)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return createdAt.DayOfWeek == DayOfWeek.Sunday ? amount * PricingRules.SundayRate : 0m;
    }

    /// <summary>
    /// Визначає сервісний збір: він стягується лише з невеликих замовлень.
    /// </summary>
    /// <param name="amount">Сума замовлення після знижок.</param>
    /// <param name="serviceFee">Розмір сервісного збору, грн.</param>
    /// <returns>Збір до сплати, грн.</returns>
    public static decimal ServiceFeeOf(decimal amount, decimal serviceFee)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(serviceFee);
        return amount < PricingRules.FreeServiceFeeFrom ? serviceFee : 0m;
    }

    /// <summary>
    /// Визначає наступний стан замовлення після розрахунку.
    /// </summary>
    /// <param name="current">Поточний стан замовлення.</param>
    /// <param name="total">Сума до сплати.</param>
    /// <returns>Новий стан; якщо перехід не передбачено — поточний.</returns>
    public static TicketStatus NextStatusOf(TicketStatus current, decimal total) => current switch
    {
        TicketStatus.New when total > 0m => TicketStatus.Paid,
        TicketStatus.Paid => TicketStatus.Used,
        _ => current,
    };
}
