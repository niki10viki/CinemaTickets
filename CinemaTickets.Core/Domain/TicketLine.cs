namespace CinemaTickets.Core.Domain;

/// <summary>Рядок замовлення: одне місце в залі, ціна й знижка.</summary>
public sealed class TicketLine
{
    public int ScreeningId { get; init; }

    public int Row { get; init; }

    public int Seat { get; init; }

    public decimal Price { get; init; }

    public decimal Discount { get; init; }

    public decimal Amount => Price - Discount;
}
