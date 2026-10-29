using CinemaTickets.Core.Abstractions;
using CinemaTickets.Core.Domain;

namespace CinemaTickets.Core.Pricing;

/// <summary>
/// Студентська ціна: фіксована знижка на кожен квиток. Нове правило — новий клас.
/// </summary>
public sealed class StudentPricingPolicy : IPricingPolicy
{
    /// <summary>Частка студентської знижки.</summary>
    private const decimal StudentRate = 0.10m;

    /// <inheritdoc/>
    public decimal PriceOf(TicketLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        return Math.Round(line.Amount * (1m - StudentRate), 2);
    }
}
