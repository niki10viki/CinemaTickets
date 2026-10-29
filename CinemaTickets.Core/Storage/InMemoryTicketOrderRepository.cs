using CinemaTickets.Core.Abstractions;
using CinemaTickets.Core.Domain;

namespace CinemaTickets.Core.Storage;

/// <summary>
/// Сховище замовлень у пам'яті на словнику за ідентифікатором.
/// </summary>
public sealed class InMemoryTicketOrderRepository : ITicketOrderRepository
{
    private readonly Dictionary<int, TicketOrder> _orders = new();

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Замовлення не передано.</exception>
    /// <exception cref="InvalidOperationException">Замовлення з таким Id уже є.</exception>
    public void Add(TicketOrder order)
    {
        ArgumentNullException.ThrowIfNull(order);
        if (!_orders.TryAdd(order.Id, order))
        {
            throw new InvalidOperationException($"Замовлення № {order.Id} вже збережено.");
        }
    }

    /// <inheritdoc/>
    public TicketOrder? GetById(int id) => _orders.GetValueOrDefault(id);

    /// <inheritdoc/>
    /// <remarks>Повертає копію, щоб клієнт не змінив вміст сховища.</remarks>
    public IReadOnlyList<TicketOrder> GetAll() => _orders.Values.ToList();
}
