using System.Globalization;
using CinemaTickets.Core.Abstractions;
using CinemaTickets.Core.Domain;
using CinemaTickets.Core.Reporting;

namespace CinemaTickets.Core.Services;

/// <summary>
/// Прикладний сервіс замовлень: сценарій оформлення та розрахунок суми за політикою.
/// </summary>
public sealed class TicketOrderService
{
    private readonly ITicketOrderRepository _repository;
    private readonly IPricingPolicy _pricing;
    private readonly INotifier _notifier;

    /// <summary>
    /// Створює сервіс із переданими залежностями; сам нічого не створює.
    /// </summary>
    /// <param name="repository">Сховище замовлень.</param>
    /// <param name="pricing">Політика ціноутворення.</param>
    /// <param name="notifier">Сповіщувач глядачів.</param>
    /// <exception cref="ArgumentNullException">Залежність не передано.</exception>
    public TicketOrderService(
        ITicketOrderRepository repository,
        IPricingPolicy pricing,
        INotifier notifier)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _pricing = pricing ?? throw new ArgumentNullException(nameof(pricing));
        _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
    }

    /// <summary>
    /// Оформлює замовлення: зберігає його у сховищі й сповіщає глядача.
    /// </summary>
    /// <param name="order">Замовлення квитків.</param>
    /// <exception cref="ArgumentNullException">Замовлення не передано.</exception>
    public void Place(TicketOrder order)
    {
        ArgumentNullException.ThrowIfNull(order);
        _repository.Add(order);
        _notifier.Notify(
            order.ViewerId,
            string.Create(
                CultureInfo.InvariantCulture,
                $"Замовлення № {order.Id} оформлено, сума {TotalOf(order):F2} грн"));
    }

    /// <summary>
    /// Рахує суму замовлення як суму цін квитків за поточною політикою.
    /// </summary>
    /// <param name="order">Замовлення квитків.</param>
    /// <returns>Сума до сплати, грн.</returns>
    /// <exception cref="ArgumentNullException">Замовлення не передано.</exception>
    public decimal TotalOf(TicketOrder order)
    {
        ArgumentNullException.ThrowIfNull(order);
        decimal total = 0m;
        foreach (TicketLine line in order.Lines)
        {
            total += _pricing.PriceOf(line);
        }

        return total;
    }

    /// <summary>
    /// Готує підсумок замовлення для звітів.
    /// </summary>
    /// <param name="order">Замовлення квитків.</param>
    /// <returns>Номер, стан і сума замовлення.</returns>
    /// <exception cref="ArgumentNullException">Замовлення не передано.</exception>
    public OrderSummary SummaryOf(TicketOrder order)
    {
        ArgumentNullException.ThrowIfNull(order);
        return new OrderSummary(order.Id, order.Status, TotalOf(order));
    }
}
