namespace CinemaTickets.Core.Legacy;

public interface ILegacyTicketStore
{
    void Add(LegacyTicketOrder order);
    LegacyTicketOrder GetById(int id);
    void SaveToFile(LegacyTicketOrder order, string path);
    void SendEmail(string to, string body);
    string ExportCsv();
    void Backup(string folder);
    void PrintInvoice(LegacyTicketOrder order);
}
