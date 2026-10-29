namespace CinemaTickets.Core.Legacy;

public sealed class SeatLine
{
    public string SeatCategory { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
