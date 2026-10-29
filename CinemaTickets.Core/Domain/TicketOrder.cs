namespace CinemaTickets.Core.Domain;

/// <summary>Замовлення квитків глядача.</summary>
public sealed class TicketOrder
{
    private readonly List<TicketLine> _lines = new();

    /// <summary>Ідентифікатор замовлення.</summary>
    public int Id { get; init; }

    /// <summary>Ідентифікатор глядача, який оформив замовлення.</summary>
    public int ViewerId { get; init; }

    /// <summary>Поточний стан замовлення.</summary>
    public TicketStatus Status { get; set; } = TicketStatus.New;

    /// <summary>Дата й час створення замовлення.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Рядки замовлення (лише для читання).</summary>
    public IReadOnlyList<TicketLine> Lines => _lines;

    /// <summary>Додає рядок із квитком до замовлення.</summary>
    /// <param name="line">Рядок замовлення, який потрібно додати.</param>
    public void AddLine(TicketLine line) => _lines.Add(line);

    /// <summary>Обчислює суму замовлення як суму вартості всіх квитків.</summary>
    /// <returns>Сума замовлення з урахуванням знижок.</returns>
    public decimal Total()
    {
        decimal sum = 0m;
        foreach (TicketLine line in _lines)
        {
            sum += line.Amount;
        }

        return sum;
    }
}
