using System.Text;

namespace CinemaTickets.Legacy;

public sealed class ReportBuilder
{
    public string Build(int orderId, string viewerName,
        IReadOnlyList<TicketItem> items, string currency,
        decimal discount, decimal shipping, decimal total)
    {
        var sb = new StringBuilder();
        sb.Append("Документ #").Append(orderId)
            .Append('\n');
        sb.Append("Клієнт: ").Append(viewerName)
            .Append('\n');
        foreach (var it in items)
            sb.Append(Line(it, currency));
        sb.Append(Money("Знижка", discount, currency));
        sb.Append(Money("Доставка", shipping, currency));
        sb.Append(Money("Разом", total, currency));
        return sb.ToString();
    }

    private static string Line(TicketItem it, string cur) =>
        it.Seat + " x" + it.Qty + " = " +
        (it.Qty * it.Price).ToString("0.00") + " " + cur +
        "\n";

    private static string Money(string label, decimal v,
        string cur) =>
        label + ": " + v.ToString("0.00") + " " + cur + "\n";
}
