namespace CinemaTickets.Core.Domain;

/// <summary>
/// Архівне замовлення: не є замовленням, а лише показує його дані тільки для читання.
/// </summary>
public sealed class ArchivedTicketOrder
{
    private readonly TicketOrder _order;

    /// <summary>
    /// Поміщає замовлення в архів.
    /// </summary>
    /// <param name="order">Замовлення, яке архівується.</param>
    /// <param name="archivedAt">Дата архівування.</param>
    /// <exception cref="ArgumentNullException">Замовлення не передано.</exception>
    public ArchivedTicketOrder(TicketOrder order, DateOnly archivedAt)
    {
        ArgumentNullException.ThrowIfNull(order);
        _order = order;
        ArchivedAt = archivedAt;
    }

    /// <summary>Дата архівування.</summary>
    public DateOnly ArchivedAt { get; }

    /// <summary>Ідентифікатор замовлення.</summary>
    public int Id => _order.Id;

    /// <summary>Стан замовлення на момент архівування.</summary>
    public TicketStatus Status => _order.Status;

    /// <summary>Квитки замовлення; методу додавання немає, тож порушити контракт нічим.</summary>
    public IReadOnlyList<TicketLine> Lines => _order.Lines;
}
