using CinemaTickets.Core.Domain;

namespace CinemaTickets.Core.Abstractions;

/// <summary>
/// Політика ціноутворення: скільки глядач платить за один квиток.
/// </summary>
public interface IPricingPolicy
{
    /// <summary>Рахує ціну квитка за правилом політики.</summary>
    /// <param name="line">Квиток на місце в залі.</param>
    /// <returns>Ціна до сплати, грн.</returns>
    decimal PriceOf(TicketLine line);
}
