namespace CinemaTickets.Core.Legacy;

public class LegacyTicketManager
{
    private readonly FileTicketStore _store = new FileTicketStore();
    private readonly SmtpMailer _mailer = new SmtpMailer();

    public decimal Place(LegacyTicketOrder order, string viewerType)
    {
        decimal total = 0m;
        foreach (var line in order.Lines)
            total += line.UnitPrice * line.Quantity;

        if (viewerType == "loyalty")
            total = total * 0.95m;
        else if (viewerType == "student")
            total = total * 0.90m;
        else if (viewerType == "staff")
            total = total * 0.70m;

        if (total > 1000m)
            total = total - 50m;

        order.Total = total;
        order.Status = 1;
        _store.SaveToFile(order, "orders.json");
        _mailer.Send(order.Email, "Order " + order.Id, "" + total);
        return total;
    }

    public string BuildCsvReport(List<LegacyTicketOrder> orders)
    {
        var text = "id;total;status" + Environment.NewLine;
        foreach (var o in orders)
            text += o.Id + ";" + o.Total + ";" + o.Status;

        File.WriteAllText("report.csv", text);
        Console.WriteLine("Звіт збережено");
        return text;
    }

    public void Archive(LegacyTicketOrder order)
    {
        LegacyTicketOrder archived = new ArchivedTicketOrder(order.Id);
        foreach (var line in order.Lines)
            archived.AddLine(line);

        _store.SaveToFile(archived, "archive.json");
    }
}
