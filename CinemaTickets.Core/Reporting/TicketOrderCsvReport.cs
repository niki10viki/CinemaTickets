using System.Globalization;
using System.Text;

namespace CinemaTickets.Core.Reporting;

/// <summary>
/// Форматувальник звіту CSV; повертає рядок і нічого не пише на диск.
/// </summary>
public static class TicketOrderCsvReport
{
    /// <summary>
    /// Будує CSV-звіт за підсумками замовлень.
    /// </summary>
    /// <param name="rows">Підсумки замовлень.</param>
    /// <returns>Текст CSV із заголовком id;total;status.</returns>
    /// <exception cref="ArgumentNullException">Підсумки не передано.</exception>
    public static string BuildCsv(IEnumerable<OrderSummary> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);
        StringBuilder text = new();
        text.AppendLine("id;total;status");
        foreach (OrderSummary row in rows)
        {
            text.AppendLine(CultureInfo.InvariantCulture, $"{row.Id};{row.Total:F2};{row.Status}");
        }

        return text.ToString();
    }
}
