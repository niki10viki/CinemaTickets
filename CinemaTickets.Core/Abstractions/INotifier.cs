namespace CinemaTickets.Core.Abstractions;

/// <summary>
/// Сповіщення глядача; спосіб доставки (пошта, консоль, SMS) знає лише реалізація.
/// </summary>
public interface INotifier
{
    /// <summary>Надсилає глядачеві повідомлення.</summary>
    /// <param name="viewerId">Ідентифікатор глядача.</param>
    /// <param name="message">Текст повідомлення.</param>
    void Notify(int viewerId, string message);
}
