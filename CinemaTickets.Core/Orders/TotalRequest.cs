using CinemaTickets.Core.Domain;

namespace CinemaTickets.Core.Orders;

/// <summary>
/// Запит на розрахунок суми замовлення квитків.
/// </summary>
/// <param name="OrderId">Номер замовлення.</param>
/// <param name="Viewer">Глядач, який оплачує замовлення.</param>
/// <param name="Lines">Рядки замовлення.</param>
/// <param name="CreatedAt">Дата створення замовлення.</param>
/// <param name="CouponCode">Промокод або null.</param>
/// <param name="Status">Поточний стан замовлення.</param>
/// <param name="ServiceFee">Сервісний збір для невеликих замовлень, грн.</param>
public sealed record TotalRequest(
    int OrderId,
    ViewerInfo Viewer,
    IReadOnlyList<SeatLine> Lines,
    DateOnly CreatedAt,
    string? CouponCode,
    TicketStatus Status,
    decimal ServiceFee);
