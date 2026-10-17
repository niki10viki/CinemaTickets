namespace CinemaTickets.Core.Domain;

/// <summary>Стан замовлення квитків у його життєвому циклі.</summary>
public enum TicketStatus
{
    New = 0,
    Paid = 1,
    Used = 2,
    Cancelled = 3,
    Refunded = 4,
}
