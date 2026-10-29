namespace CinemaTickets.Core.Legacy;

public class LegacyTicketLine
{
    public string SeatCategory { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class LegacyTicketOrder
{
    public int Id { get; set; }
    public int Status { get; set; }
    public decimal Total { get; set; }
    public string Email { get; set; } = "";
    public List<LegacyTicketLine> Lines { get; set; } = new();

    public virtual void AddLine(LegacyTicketLine line) => Lines.Add(line);
}

public class ArchivedTicketOrder : LegacyTicketOrder
{
    public ArchivedTicketOrder(int id) => Id = id;

    public override void AddLine(LegacyTicketLine line)
    {
        throw new NotSupportedException("Архів змінювати не можна");
    }
}
