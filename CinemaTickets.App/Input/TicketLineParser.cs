using System.Globalization;
using CinemaTickets.Core.Domain;
using CinemaTickets.Core.Errors;

namespace CinemaTickets.App.Input;

/// <summary>
/// Барикада: перетворює рядки, введені в консолі, на перевірений квиток.
/// </summary>
internal static class TicketLineParser
{
    /// <summary>Найбільший номер ряду в залі.</summary>
    public const int MaxRow = 30;

    /// <summary>Найбільший номер місця в ряду.</summary>
    public const int MaxSeat = 40;

    /// <summary>Найвища ціна квитка, грн: більша сума — майже напевно помилка введення.</summary>
    public const decimal MaxPrice = 10_000m;

    /// <summary>Найдовший рядок введення, символів.</summary>
    public const int MaxLineLength = 40;

    /// <summary>
    /// Розбирає рядок «ряд місце ціна», введений у консолі.
    /// </summary>
    /// <param name="screeningId">Ідентифікатор поточного сеансу.</param>
    /// <param name="raw">Рядок введення.</param>
    /// <returns>Квиток або пояснення помилки введення.</returns>
    public static Result<TicketLine> Parse(int screeningId, string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw) || raw.Length > MaxLineLength)
        {
            return Result<TicketLine>.Fail("Введіть ряд, місце й ціну через пробіл");
        }

        string[] parts = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
        {
            return Result<TicketLine>.Fail("Потрібно три значення — ряд, місце, ціна");
        }

        return Parse(screeningId, parts[0], parts[1], parts[2]);
    }

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
        if (!int.TryParse(row, NumberStyles.Integer, culture, out int r) || r is < 1 or > MaxRow)
        {
            return Result<TicketLine>.Fail("Ряд — ціле число від 1 до 30");
        }

        if (!int.TryParse(seat, NumberStyles.Integer, culture, out int s)
            || s is < 1 or > MaxSeat)
        {
            return Result<TicketLine>.Fail("Місце — ціле число від 1 до 40");
        }

        if (!decimal.TryParse(price, NumberStyles.Number, culture, out decimal p)
            || p is < 0m or > MaxPrice)
        {
            return Result<TicketLine>.Fail(
                "Ціна — число від 0 до 10 000, дробова частина через кому");
        }

        return Result<TicketLine>.Ok(new TicketLine(screeningId, r, s, p));
    }

    /// <summary>
    /// Рахує рядки, у яких записано додатну кількість квитків.
    /// </summary>
    /// <param name="rows">Рядки з кількістю квитків.</param>
    /// <returns>Кількість коректних рядків.</returns>
    public static int CountValidQuantities(string[] rows)
    {
        ArgumentNullException.ThrowIfNull(rows);
        var valid = 0;
        foreach (var row in rows)
        {
            // «не число» — звичайна ситуація, тому її перевіряє умова, а не виняток
            bool isNumber = int.TryParse(
                row, NumberStyles.Integer, CultureInfo.CurrentCulture, out int q);
            if (isNumber && q > 0)
            {
                valid++;
            }
        }

        return valid;
    }
}
