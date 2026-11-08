using System.Globalization;
using CinemaTickets.Core.Abstractions;
using CinemaTickets.Core.Domain;
using CinemaTickets.Core.Errors;
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
    /// Додає перевірений квиток до нового замовлення.
    /// </summary>
    /// <param name="orderId">Номер замовлення.</param>
    /// <param name="line">Квиток, уже розібраний і перевірений за барикадою.</param>
    /// <exception cref="ArgumentNullException">Квиток не передано.</exception>
    /// <exception cref="DomainRuleException">
    /// Замовлення немає, воно вже не нове або місце в ньому вже є.
    /// </exception>
    public void AddLine(int orderId, TicketLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        TicketOrder order = Find(orderId);
        if (order.Status != TicketStatus.New)
        {
            throw new DomainRuleException(
                "order.editable",
                Text($"Замовлення {orderId} у стані {order.Status}: квитки лише до нового"));
        }

        bool seatTaken = order.Lines.Any(
            l => l.ScreeningId == line.ScreeningId && l.Row == line.Row && l.Seat == line.Seat);
        if (seatTaken)
        {
            throw new DomainRuleException(
                "seat.taken",
                Text($"Місце {line.Row}/{line.Seat} уже є в замовленні {orderId}"));
        }

        order.AddLine(line);
    }

    /// <summary>
    /// Оплачує нове замовлення внесеною сумою.
    /// </summary>
    /// <param name="orderId">Номер замовлення.</param>
    /// <param name="amount">Внесена сума, грн.</param>
    /// <exception cref="DomainRuleException">
    /// Замовлення немає, воно не нове або внесено менше, ніж треба.
    /// </exception>
    public void Pay(int orderId, decimal amount)
    {
        TicketOrder order = Find(orderId);
        if (order.Status != TicketStatus.New)
        {
            throw new DomainRuleException(
                "order.payable",
                Text($"Стан {order.Status} замовлення {orderId} не дозволяє оплату"));
        }

        decimal total = TotalOf(order);
        if (amount < total)
        {
            throw new DomainRuleException(
                "order.amount",
                Text($"Сплачено {amount:0.00}, треба {total:0.00}"));
        }

        order.Pay();
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

    // повідомлення бачить користувач, тому числа форматуються за його культурою
    private static string Text(FormattableString message) =>
        message.ToString(CultureInfo.CurrentCulture);

    private TicketOrder Find(int orderId) =>
        _repository.GetById(orderId)
        ?? throw new DomainRuleException("order.exists", Text($"Замовлення {orderId} не знайдено"));
}
