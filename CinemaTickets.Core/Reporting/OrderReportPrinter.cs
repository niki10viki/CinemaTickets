namespace CinemaTickets.Core.Reporting;

/// <summary>
/// Виведення готового звіту в переданий потік; про консоль клас не знає.
/// </summary>
public sealed class OrderReportPrinter
{
    private readonly TextWriter _output;

    /// <summary>
    /// Створює принтер, що пише в заданий потік.
    /// </summary>
    /// <param name="output">Потік виводу: стандартний вивід застосунку або StringWriter у тесті.</param>
    /// <exception cref="ArgumentNullException">Потік не передано.</exception>
    public OrderReportPrinter(TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(output);
        _output = output;
    }

    /// <summary>
    /// Записує звіт у потік виводу; команда, нічого не повертає.
    /// </summary>
    /// <param name="report">Текст звіту.</param>
    /// <exception cref="ArgumentException">Текст звіту порожній.</exception>
    public void Print(string report)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(report);
        _output.Write(report);
    }
}
