using System.Globalization;
using CinemaTickets.Core.Errors;

namespace CinemaTickets.App.Input;

/// <summary>
/// Барикада для суми оплати, введеної в консолі.
/// </summary>
internal static class AmountParser
{
    /// <summary>
    /// Розбирає суму оплати.
    /// </summary>
    /// <param name="text">Сума у вигляді рядка; дробова частина — за культурою системи.</param>
    /// <returns>Сума або пояснення помилки введення.</returns>
    public static Result<decimal> Parse(string? text)
    {
        bool isNumber = decimal.TryParse(
            text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal amount);
        if (!isNumber || amount <= 0)
        {
            return Result<decimal>.Fail("Сума — додатне число, дробова частина через кому");
        }

        return Result<decimal>.Ok(amount);
    }
}
