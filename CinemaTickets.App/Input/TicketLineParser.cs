using System.Globalization;
using CinemaTickets.Core.Domain;
using CinemaTickets.Core.Errors;

namespace CinemaTickets.App.Input;

/// <summary>
/// Барикада: перетворює рядки, введені в консолі, на перевірений квиток.
/// </summary>
internal static class TicketLineParser
{
    /// <summary>
    /// Розбирає ряд, місце й ціну квитка.
    /// </summary>
    /// <param name="screeningId">Ідентифікатор поточного сеансу.</param>
    /// <param name="row">Ряд у вигляді рядка.</param>
    /// <param name="seat">Місце у вигляді рядка.</param>
    /// <param name="price">Ціна у вигляді рядка; дробова частина — за культурою системи.</param>
    /// <returns>Квиток або пояснення помилки введення.</returns>
    public static Result<TicketLine> Parse(
        int screeningId,
        string? row,
        string? seat,
        string? price)
    {
        CultureInfo culture = CultureInfo.CurrentCulture;
        if (!int.TryParse(row, NumberStyles.Integer, culture, out int r) || r <= 0)
        {
            return Result<TicketLine>.Fail("Ряд — ціле число більше 0");
        }

        if (!int.TryParse(seat, NumberStyles.Integer, culture, out int s) || s <= 0)
        {
            return Result<TicketLine>.Fail("Місце — ціле число більше 0");
        }

        if (!decimal.TryParse(price, NumberStyles.Number, culture, out decimal p) || p < 0)
        {
            return Result<TicketLine>.Fail("Ціна — невід'ємне число, дробова частина через кому");
        }

        return Result<TicketLine>.Ok(new TicketLine(screeningId, r, s, p));
    }
}
