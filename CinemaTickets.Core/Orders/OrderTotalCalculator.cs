using CinemaTickets.Core.Domain;

namespace CinemaTickets.Core.Orders;

/// <summary>
/// Розрахунок суми замовлення квитків, розкладений на кроки з однією відповідальністю.
/// </summary>
public static class OrderTotalCalculator
{
    /// <summary>
    /// Рахує замовлення: перевіряє вхід, застосовує знижки й збір, визначає новий стан.
    /// </summary>
    /// <param name="orderId">Номер замовлення.</param>
    /// <param name="viewerName">Ім'я глядача.</param>
    /// <param name="viewerEmail">Пошта глядача.</param>
    /// <param name="isRegular">Ознака постійного глядача.</param>
    /// <param name="lines">Рядки замовлення.</param>
    /// <param name="createdAt">Дата створення замовлення.</param>
    /// <param name="couponCode">Промокод або порожній рядок.</param>
    /// <param name="status">Поточний стан замовлення.</param>
    /// <param name="printToConsole">Чи друкувати звіт у консоль.</param>
    /// <param name="serviceFee">Сервісний збір для невеликих замовлень, грн.</param>
    /// <param name="newStatus">Стан замовлення після розрахунку.</param>
    /// <returns>Сума до сплати, заокруглена до двох знаків.</returns>
    public static decimal Process(
        int orderId,
        string viewerName,
        string viewerEmail,
        bool isRegular,
        IReadOnlyList<SeatLine> lines,
        DateOnly createdAt,
        string couponCode,
        TicketStatus status,
        bool printToConsole,
        decimal serviceFee,
        out TicketStatus newStatus)
    {
        OrderGuards.EnsureHeaderValid(orderId, viewerName, viewerEmail);
        decimal total = SubtotalOf(lines);
        total -= ViewerDiscountOf(total, isRegular);
        total -= VolumeDiscountOf(total);
        total -= CouponDiscountOf(total, couponCode);
        total -= SundayDiscountOf(total, createdAt);
        total += ServiceFeeOf(total, serviceFee);
        total = status == TicketStatus.Cancelled
            ? 0m
            : Math.Round(Math.Max(total, 0m), 2);
        newStatus = NextStatusOf(status, total);
        if (printToConsole)
        {
            Console.WriteLine("Замовлення № " + orderId);
            Console.WriteLine("Глядач: " + viewerName);
            Console.WriteLine("Пошта: " + viewerEmail);
            foreach (SeatLine line in lines)
            {
                Console.WriteLine(line.SeatCategory + " x " + line.Quantity);
            }

            Console.WriteLine("Разом: " + total);
            Console.WriteLine("Стан: " + newStatus);
        }

        return total;
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
