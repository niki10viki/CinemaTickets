using CinemaTickets.Core.Domain;

namespace CinemaTickets.Core.Orders;

/// <summary>
/// Результат розрахунку замовлення.
/// </summary>
/// <param name="Total">Сума до сплати, заокруглена до двох знаків.</param>
/// <param name="Status">Стан замовлення після розрахунку.</param>
public sealed record TotalResult(
    decimal Total,
    TicketStatus Status);
