using System.Globalization;
using CinemaTickets.Core.Abstractions;
using CinemaTickets.Core.Domain;
using CinemaTickets.Core.Errors;
using CinemaTickets.Core.Reporting;
using Microsoft.Extensions.Logging;

namespace CinemaTickets.Core.Services;

/// <summary>
/// Прикладний сервіс замовлень: сценарій оформлення та розрахунок суми за політикою.
/// </summary>
public sealed class TicketOrderService
{
    private readonly ITicketOrderRepository _repository;
    private readonly IPricingPolicy _pricing;
    private readonly INotifier _notifier;
    private readonly ILogger<TicketOrderService> _log;

    /// <summary>
    /// Створює сервіс із переданими залежностями; сам нічого не створює.
    /// </summary>
    /// <param name="repository">Сховище замовлень.</param>
    /// <param name="pricing">Політика ціноутворення.</param>
    /// <param name="notifier">Сповіщувач глядачів.</param>
    /// <param name="log">Журнал сервісу; ядро не пише в консоль саме.</param>
    /// <exception cref="ArgumentNullException">Залежність не передано.</exception>
    public TicketOrderService(
        ITicketOrderRepository repository,
        IPricingPolicy pricing,
        INotifier notifier,
        ILogger<TicketOrderService> log)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _pricing = pricing ?? throw new ArgumentNullException(nameof(pricing));
        _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
        _log = log ?? throw new ArgumentNullException(nameof(log));
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
        _log.LogInformation(
            "Замовлення {OrderId} оформлено, квитків {Count}", order.Id, order.Lines.Count);
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
        using IDisposable? scope = _log.BeginScope("Order:{OrderId}", orderId);
        _log.LogDebug(
            "Додавання місця {Row}/{Seat}, ціна {Price}", line.Row, line.Seat, line.Price);
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
            _log.LogWarning(
                "Місце {Row}/{Seat} уже є в замовленні, відхилено", line.Row, line.Seat);
            throw new DomainRuleException(
                "seat.taken",
                Text($"Місце {line.Row}/{line.Seat} уже є в замовленні {orderId}"));
        }

        order.AddLine(line);
        _log.LogInformation("Додано місце {Row}/{Seat}", line.Row, line.Seat);
    }

    /// <summary>
    /// Оплачує нове замовлення внесеною сумою.
    /// </summary>
    /// <param name="orderId">Номер замовлення.</param>
    /// <param name="amount">Внесена сума, грн.</param>
    /// <exception cref="DomainRuleException">
    /// Замовлення немає, воно не нове, порожнє або внесено менше, ніж треба.
    /// </exception>
    public void Pay(int orderId, decimal amount)
    {
        using IDisposable? scope = _log.BeginScope("Order:{OrderId}", orderId);
        _log.LogDebug("Початок оплати, внесено {Amount}", amount);
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
            _log.LogWarning("Оплату відхилено: внесено {Amount}, треба {Total}", amount, total);
            throw new DomainRuleException(
                "order.amount",
                Text($"Сплачено {amount:0.00}, треба {total:0.00}"));
        }

        if (amount > total)
        {
            _log.LogWarning("Переплата {Extra}, касир видає решту", amount - total);
        }

        try
        {
            order.Pay();
        }
        catch (InvalidOperationException ex)
        {
            // діалог не пускає до оплати порожнє замовлення, тож це дефект коду, що викликає сервіс
            _log.LogError(ex, "До оплати дійшло замовлення без квитків");
            throw new DomainRuleException(
                "order.empty",
                Text($"Замовлення {orderId} не містить квитків"),
                ex);
        }

        _log.LogInformation("Оплачено {Total}, стан {Status}", total, order.Status);
    }

    /// <summary>
    /// Завантажує замовлення за номером.
    /// </summary>
    /// <param name="orderId">Номер замовлення.</param>
    /// <returns>Знайдене замовлення.</returns>
    /// <exception cref="DomainRuleException">Замовлення не знайдено.</exception>
    public TicketOrder Load(int orderId) => Find(orderId);

    /// <summary>
    /// Скасовує нове замовлення.
    /// </summary>
    /// <param name="orderId">Номер замовлення.</param>
    /// <param name="reason">Причина скасування.</param>
    /// <exception cref="DomainRuleException">
    /// Замовлення немає або його стан не дозволяє скасування.
    /// </exception>
    public void Cancel(int orderId, string reason)
    {
        using IDisposable? scope = _log.BeginScope("Order:{OrderId}", orderId);
        TicketOrder order = Find(orderId);
        try
        {
            order.Cancel();
            _log.LogInformation("Замовлення скасовано, причина: {Reason}", reason);
        }
        catch (InvalidOperationException ex)
        {
            // перехоплюємо лише очікуваний виняток і загортаємо його, зберігаючи першопричину
            _log.LogWarning(ex, "Скасування відхилено, стан {Status}", order.Status);
            throw new DomainRuleException(
                "order.cancellable",
                Text($"Замовлення {orderId} у стані {order.Status} не можна скасувати"),
                ex);
        }
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
    /// Зберігає звіт про замовлення в текстовий файл.
    /// </summary>
    /// <param name="path">Шлях до файлу звіту.</param>
    /// <param name="order">Замовлення квитків.</param>
    /// <exception cref="ArgumentNullException">Замовлення не передано.</exception>
    /// <exception cref="IOException">Файл не вдалося створити або записати.</exception>
    public void ExportReport(string path, TicketOrder order)
    {
        ArgumentNullException.ThrowIfNull(order);
        using IDisposable? scope = _log.BeginScope("Order:{OrderId}", order.Id);
        try
        {
            // using закриває файл навіть тоді, коли TotalOf або запис кинуть виняток
            using var writer = new StreamWriter(path);
            writer.WriteLine(Text($"Замовлення {order.Id}, стан {order.Status}"));
            writer.WriteLine(Text($"Квитків: {order.Lines.Count}"));
            writer.WriteLine(Text($"До сплати: {TotalOf(order):0.00} грн"));
        }
        catch (IOException ex)
        {
            // збій середовища: записуємо в журнал і передаємо вгору зі збереженим стеком
            _log.LogError(ex, "Звіт не записано у {Path}", path);
            throw;
        }

        _log.LogInformation("Звіт збережено у {Path}", path);
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
