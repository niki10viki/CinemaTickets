using CinemaTickets.Core.Abstractions;
using CinemaTickets.Core.Domain;

namespace CinemaTickets.Core.Pricing;

/// <summary>
/// Знижка на кожен квиток у заданій частці (картка лояльності, працівник тощо).
/// </summary>
public sealed class DiscountPricingPolicy : IPricingPolicy
{
    /// <summary>Найбільша допустима частка знижки.</summary>
    private const decimal MaxRate = 0.5m;

    private readonly decimal _rate;

    /// <summary>
    /// Створює політику зі знижкою у вказаній частці.
    /// </summary>
    /// <param name="rate">Частка знижки від 0 до 0,5.</param>
    /// <exception cref="ArgumentOutOfRangeException">Частка поза межами 0–0,5.</exception>
    public DiscountPricingPolicy(decimal rate)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(rate);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(rate, MaxRate);
        _rate = rate;
    }

    /// <inheritdoc/>
    public decimal PriceOf(TicketLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        return Math.Round(line.Amount * (1m - _rate), 2);
    }
}
