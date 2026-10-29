namespace CinemaTickets.Core.Orders;

/// <summary>Рядок замовлення: категорія місць, кількість квитків і ціна одного квитка.</summary>
public sealed class SeatLine
{
    /// <summary>Категорія місць у залі, наприклад STANDARD або VIP.</summary>
    public string SeatCategory { get; set; } = string.Empty;

    /// <summary>Кількість квитків цієї категорії.</summary>
    public int Quantity { get; set; }

    /// <summary>Ціна одного квитка, грн.</summary>
    public decimal UnitPrice { get; set; }
}
