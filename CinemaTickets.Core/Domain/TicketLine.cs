namespace CinemaTickets.Core.Domain;

/// <summary>Рядок замовлення: одне місце в залі, ціна й знижка.</summary>
public sealed class TicketLine
{
    /// <summary>Ідентифікатор сеансу, на який куплено квиток.</summary>
    public int ScreeningId { get; init; }

    /// <summary>Номер ряду в залі.</summary>
    public int Row { get; init; }

    /// <summary>Номер місця в ряду.</summary>
    public int Seat { get; init; }

    /// <summary>Ціна квитка до знижки, грн.</summary>
    public decimal Price { get; init; }

    /// <summary>Знижка на квиток, грн.</summary>
    public decimal Discount { get; init; }

    /// <summary>Вартість квитка з урахуванням знижки, грн.</summary>
    public decimal Amount => Price - Discount;
}
