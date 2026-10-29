using CinemaTickets.Core.Domain;

namespace CinemaTickets.Core.Abstractions;

/// <summary>
/// Сховище замовлень квитків; відповідає лише за зберігання.
/// </summary>
public interface ITicketOrderRepository
{
    /// <summary>Додає нове замовлення.</summary>
    /// <param name="order">Замовлення, яке потрібно зберегти.</param>
    void Add(TicketOrder order);

    /// <summary>Шукає замовлення за ідентифікатором.</summary>
    /// <param name="id">Ідентифікатор замовлення.</param>
    /// <returns>Замовлення або null, якщо його немає.</returns>
    TicketOrder? GetById(int id);

    /// <summary>Повертає всі збережені замовлення.</summary>
    /// <returns>Список замовлень лише для читання.</returns>
    IReadOnlyList<TicketOrder> GetAll();
}
