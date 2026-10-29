using System.Globalization;
using System.Text;
using CinemaTickets.Core.Orders;

namespace CinemaTickets.Core.Reporting;

/// <summary>
/// Формування текстового звіту за розрахованим замовленням.
/// </summary>
public static class OrderReport
{
    /// <summary>
    /// Будує текст звіту; чиста функція: результат залежить лише від аргументів.
    /// </summary>
    /// <param name="request">Запит, за яким рахувалося замовлення.</param>
    /// <param name="result">Результат розрахунку замовлення.</param>
    /// <returns>Багаторядковий текст звіту.</returns>
    /// <exception cref="ArgumentNullException">Запит або результат не передано.</exception>
    public static string TextOf(TotalRequest request, TotalResult result)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(result);

        // культура задана явно: інакше текст залежав би від налаштувань системи
        CultureInfo culture = CultureInfo.InvariantCulture;
        StringBuilder text = new();
        text.AppendLine(culture, $"Замовлення № {request.OrderId}");
        text.AppendLine(culture, $"Глядач: {request.Viewer.Name}");
        text.AppendLine(culture, $"Пошта: {request.Viewer.Email}");
        foreach (SeatLine line in request.Lines)
        {
            text.AppendLine(culture, $"{line.SeatCategory} x {line.Quantity}");
        }

        text.AppendLine(culture, $"Разом: {result.Total:F2}");
        text.AppendLine(culture, $"Стан: {result.Status}");
        return text.ToString();
    }
}
