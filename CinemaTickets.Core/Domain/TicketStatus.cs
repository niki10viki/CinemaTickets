namespace CinemaTickets.Core.Domain;

/// <summary>Стан замовлення квитків у його життєвому циклі.</summary>
public enum TicketStatus
{
    /// <summary>Замовлення створено, але ще не оплачено.</summary>
    New = 0,

    /// <summary>Замовлення оплачено.</summary>
    Paid = 1,

    /// <summary>Квитки використано на сеансі.</summary>
    Used = 2,

    /// <summary>Замовлення скасовано до оплати.</summary>
    Cancelled = 3,

    /// <summary>Кошти за оплачене замовлення повернуто.</summary>
    Refunded = 4,
}
