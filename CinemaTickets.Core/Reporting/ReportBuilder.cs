using System.Text;

namespace CinemaTickets.Core.Reporting;

/// <summary>
/// Формує текст простого звіту з готових рядків.
/// </summary>
public sealed class ReportBuilder
{
    /// <summary>Заголовок звіту; у тексті друкується великими літерами.</summary>
    public string Title { get; init; } = "Звіт";

    /// <summary>
    /// Складає звіт: заголовок, а під ним рядки в тому самому порядку.
    /// </summary>
    /// <param name="rows">Готові рядки звіту.</param>
    /// <returns>Текст звіту.</returns>
    /// <exception cref="ArgumentNullException">Рядки не передано.</exception>
    public string Build(IReadOnlyList<string> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        var text = new StringBuilder();
        text.AppendLine(Title.ToUpperInvariant());
        foreach (var row in rows)
        {
            text.AppendLine(row);
        }

        return text.ToString();
    }

    public string Describe(int count)
    {
        string note;
        return "Позицій: " + count;
    }
}
