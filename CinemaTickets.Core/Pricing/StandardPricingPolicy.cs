using CinemaTickets.Core.Abstractions;
using CinemaTickets.Core.Domain;

namespace CinemaTickets.Core.Pricing;

/// <summary>
/// Звичайна ціна: глядач платить вартість квитка без додаткових знижок.
/// </summary>
public sealed class StandardPricingPolicy : IPricingPolicy
{
    /// <inheritdoc/>
    public decimal PriceOf(TicketLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        return line.Amount;
    }
}
