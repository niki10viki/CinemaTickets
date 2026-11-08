using System.Text;

namespace CinemaTickets.Legacy;

public sealed class ReportBuilder
{
    public string Build(BookingRequest req, decimal discount,
        decimal shipping, decimal total)
    {
        var sb = new StringBuilder();
        sb.Append("Документ #").Append(req.OrderId)
            .Append('\n');
        sb.Append("Клієнт: ").Append(req.Viewer.Name)
            .Append('\n');
        foreach (var it in req.Items!)
            sb.Append(Line(it, req.Currency));
        sb.Append(Money("Знижка", discount, req.Currency));
        sb.Append(Money("Доставка", shipping, req.Currency));
        sb.Append(Money("Разом", total, req.Currency));
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
