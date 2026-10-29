namespace CinemaTickets.Core.Orders;

/// <summary>
/// Глядач, який оплачує замовлення.
/// </summary>
/// <param name="Name">Ім'я глядача.</param>
/// <param name="Email">Пошта глядача.</param>
/// <param name="IsRegular">Ознака постійного глядача.</param>
public sealed record ViewerInfo(
    string Name,
    string Email,
    bool IsRegular);
