namespace CinemaTickets.Core.Domain;

/// <summary>
/// Квиток на одне місце в залі: незмінна позиція замовлення з ціною та знижкою.
/// </summary>
public sealed record TicketLine
{
    /// <summary>
    /// Створює квиток і перевіряє інваріант позиції.
    /// </summary>
    /// <param name="screeningId">Ідентифікатор сеансу; додатний.</param>
    /// <param name="row">Номер ряду; додатний.</param>
    /// <param name="seat">Номер місця в ряду; додатний.</param>
    /// <param name="price">Ціна квитка до знижки, грн; не від'ємна.</param>
    /// <param name="discount">Знижка, грн; від 0 до ціни квитка.</param>
    /// <exception cref="ArgumentOutOfRangeException">Порушено інваріант позиції.</exception>
    public TicketLine(int screeningId, int row, int seat, decimal price, decimal discount = 0m)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(screeningId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(row);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(seat);
        ArgumentOutOfRangeException.ThrowIfNegative(price);
        ArgumentOutOfRangeException.ThrowIfNegative(discount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(discount, price);

        ScreeningId = screeningId;
        Row = row;
        Seat = seat;
        Price = price;
        Discount = discount;
    }

    /// <summary>Ідентифікатор сеансу, на який куплено квиток.</summary>
    public int ScreeningId { get; }

    /// <summary>Номер ряду в залі.</summary>
    public int Row { get; }

    /// <summary>Номер місця в ряду.</summary>
    public int Seat { get; }

    /// <summary>Ціна квитка до знижки, грн.</summary>
    public decimal Price { get; }

    /// <summary>Знижка на квиток, грн.</summary>
    public decimal Discount { get; }

    /// <summary>Вартість квитка з урахуванням знижки, грн.</summary>
    /// <remarks>Обчислюється, а не зберігається: розійтися з ціною та знижкою не може.</remarks>
    public decimal Amount => Price - Discount;
}
