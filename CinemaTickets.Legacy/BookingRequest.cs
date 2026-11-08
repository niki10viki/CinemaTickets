namespace CinemaTickets.Legacy;

public sealed record BookingRequest(
    int OrderId,
    Viewer Viewer,
    IReadOnlyList<TicketItem>? Items,
    DocState State,
    string Currency,
    bool SendMail);
