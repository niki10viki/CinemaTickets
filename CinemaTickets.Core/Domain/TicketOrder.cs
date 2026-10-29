namespace CinemaTickets.Core.Domain;

/// <summary>
/// Замовлення квитків глядача: агрегат, стан якого змінюють лише методи-переходи.
/// </summary>
public sealed class TicketOrder
{
    /// <summary>За скільки годин до початку сеансу ще можна повернути кошти.</summary>
    private const int RefundDeadlineHours = 1;

    private readonly List<TicketLine> _lines = new();

    /// <summary>
    /// Створює нове замовлення у стані <see cref="TicketStatus.New"/>.
    /// </summary>
    /// <param name="id">Ідентифікатор замовлення; додатний.</param>
    /// <param name="viewerId">Ідентифікатор глядача; додатний.</param>
    /// <param name="createdAt">Дата й час створення замовлення.</param>
    /// <exception cref="ArgumentOutOfRangeException">Ідентифікатор не додатний.</exception>
    public TicketOrder(int id, int viewerId, DateTimeOffset createdAt)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(viewerId);

        Id = id;
        ViewerId = viewerId;
        CreatedAt = createdAt;
        Status = TicketStatus.New;
    }

    /// <summary>Ідентифікатор замовлення.</summary>
    public int Id { get; }

    /// <summary>Ідентифікатор глядача, який оформив замовлення.</summary>
    public int ViewerId { get; }

    /// <summary>Дата й час створення замовлення.</summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>Поточний стан замовлення; змінюється лише методами-переходами.</summary>
    public TicketStatus Status { get; private set; }

    /// <summary>Квитки замовлення; переглянути можна, додати повз AddLine — ні.</summary>
    public IReadOnlyList<TicketLine> Lines => _lines;

    /// <summary>
    /// Додає квиток до нового замовлення.
    /// </summary>
    /// <param name="line">Квиток на місце в залі.</param>
    /// <exception cref="ArgumentNullException">Квиток не передано.</exception>
    /// <exception cref="InvalidOperationException">
    /// Замовлення вже не нове або це місце вже є в замовленні.
    /// </exception>
    public void AddLine(TicketLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        EnsureStatus(TicketStatus.New, "Квитки додають лише до нового замовлення.");
        if (_lines.Exists(l => IsSameSeat(l, line)))
        {
            throw new InvalidOperationException("Це місце вже є в замовленні.");
        }

        _lines.Add(line);
    }

    /// <summary>
    /// Оплачує замовлення: New → Paid.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Замовлення не нове або не містить жодного квитка.
    /// </exception>
    public void Pay()
    {
        EnsureStatus(TicketStatus.New, "Оплатити можна лише нове замовлення.");
        if (_lines.Count == 0)
        {
            throw new InvalidOperationException("Замовлення не містить жодного квитка.");
        }

        Status = TicketStatus.Paid;
    }

    /// <summary>
    /// Скасовує неоплачене замовлення: New → Cancelled.
    /// </summary>
    /// <exception cref="InvalidOperationException">Замовлення не нове.</exception>
    public void Cancel()
    {
        EnsureStatus(TicketStatus.New, "Скасувати можна лише неоплачене замовлення.");
        Status = TicketStatus.Cancelled;
    }

    /// <summary>
    /// Погашає квитки на вході в залу: Paid → Used.
    /// </summary>
    /// <exception cref="InvalidOperationException">Замовлення не оплачене.</exception>
    public void MarkUsed()
    {
        EnsureStatus(TicketStatus.Paid, "Погасити можна лише оплачені квитки.");
        Status = TicketStatus.Used;
    }

    /// <summary>
    /// Повертає кошти за оплачене замовлення: Paid → Refunded.
    /// </summary>
    /// <param name="requestedAt">Момент звернення глядача.</param>
    /// <param name="screeningStartsAt">Час початку сеансу.</param>
    /// <exception cref="InvalidOperationException">
    /// Замовлення не оплачене або до сеансу лишилося менше години.
    /// </exception>
    public void Refund(DateTimeOffset requestedAt, DateTimeOffset screeningStartsAt)
    {
        EnsureStatus(TicketStatus.Paid, "Повернути кошти можна лише за оплачене замовлення.");
        if (screeningStartsAt - requestedAt < TimeSpan.FromHours(RefundDeadlineHours))
        {
            throw new InvalidOperationException("Повернення можливе не пізніше ніж за годину.");
        }

        Status = TicketStatus.Refunded;
    }

    private static bool IsSameSeat(TicketLine left, TicketLine right) =>
        left.ScreeningId == right.ScreeningId && left.Row == right.Row && left.Seat == right.Seat;

    private void EnsureStatus(TicketStatus expected, string message)
    {
        if (Status != expected)
        {
            throw new InvalidOperationException(message);
        }
    }
}
