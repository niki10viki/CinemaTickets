using CinemaTickets.Core.Abstractions;

namespace CinemaTickets.App;

/// <summary>
/// Сповіщувач для консольного застосунку: консоль належить застосунку, а не бібліотеці.
/// </summary>
internal sealed class ConsoleNotifier : INotifier
{
    /// <inheritdoc/>
    public void Notify(int viewerId, string message) =>
        Console.WriteLine($"[глядачу {viewerId}] {message}");
}
