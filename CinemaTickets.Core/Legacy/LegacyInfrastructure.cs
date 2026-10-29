using System.Text.Json;

namespace CinemaTickets.Core.Legacy;

// Заглушки, на які посилається клас-Бог: методичка їх не наводить, вони потрібні для компіляції.
public class FileTicketStore : ILegacyTicketStore
{
    private readonly Dictionary<int, LegacyTicketOrder> _orders = new();

    public void Add(LegacyTicketOrder order) => _orders.Add(order.Id, order);

    public LegacyTicketOrder GetById(int id) => _orders[id];

    public void SaveToFile(LegacyTicketOrder order, string path) =>
        File.WriteAllText(path, JsonSerializer.Serialize(order));

    public void SendEmail(string to, string body) =>
        throw new NotSupportedException("Сховище не надсилає листів");

    public string ExportCsv() => string.Join(";", _orders.Keys);

    public void Backup(string folder) => Directory.CreateDirectory(folder);

    public void PrintInvoice(LegacyTicketOrder order) => Console.WriteLine("Рахунок " + order.Id);
}

public class SmtpMailer
{
    public void Send(string to, string subject, string body) =>
        Console.WriteLine("SMTP " + to + ": " + subject + " " + body);
}
