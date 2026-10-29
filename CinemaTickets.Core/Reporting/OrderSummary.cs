using CinemaTickets.Core.Domain;

namespace CinemaTickets.Core.Reporting;

/// <summary>
/// Рядок звіту про замовлення: номер, стан і сума за політикою.
/// </summary>
/// <param name="Id">Ідентифікатор замовлення.</param>
/// <param name="Status">Стан замовлення.</param>
/// <param name="Total">Сума до сплати, грн.</param>
public sealed record OrderSummary(int Id, TicketStatus Status, decimal Total);
