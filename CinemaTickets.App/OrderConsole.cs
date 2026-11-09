using CinemaTickets.App.Input;
using CinemaTickets.Core.Domain;
using CinemaTickets.Core.Errors;
using CinemaTickets.Core.Services;

namespace CinemaTickets.App;

/// <summary>
/// Діалог оформлення замовлення в консолі: помилки введення пояснюються, запит повторюється.
/// </summary>
internal sealed class OrderConsole
{
    private readonly TicketOrderService _service;
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly bool _echoInput;

    /// <summary>
    /// Створює діалог над сервісом і потоками введення-виведення.
    /// </summary>
    /// <param name="service">Сервіс замовлень.</param>
    /// <param name="input">Звідки читати відповіді користувача.</param>
    /// <param name="output">Куди писати запити й повідомлення.</param>
    /// <param name="echoInput">Чи повторювати введене, коли воно надходить не з клавіатури.</param>
    public OrderConsole(
        TicketOrderService service,
        TextReader input,
        TextWriter output,
        bool echoInput)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _echoInput = echoInput;
    }

    /// <summary>
    /// Проводить діалог: квитки, потім оплата.
    /// </summary>
    /// <param name="order">Нове замовлення, уже зареєстроване у сховищі.</param>
    /// <param name="screeningId">Сеанс, на який купуються квитки.</param>
    public void Run(TicketOrder order, int screeningId)
    {
        ArgumentNullException.ThrowIfNull(order);
        _output.WriteLine($"Замовлення № {order.Id}, сеанс {screeningId}.");
        _output.WriteLine("Введіть квиток: ряд місце ціна (порожній рядок — завершити).");
        if (!ReadTickets(order, screeningId) || order.Lines.Count == 0)
        {
            return;
        }

        _output.WriteLine($"До сплати: {_service.TotalOf(order):0.00} грн. Введіть суму оплати.");
        ReadPayment(order);
    }

    // false — потік введення закрито, далі питати немає кого
    private bool ReadTickets(TicketOrder order, int screeningId)
    {
        while (true)
        {
            _output.Write("> ");
            string? line = ReadLine();
            if (line is null)
            {
                _output.WriteLine("Введення завершено.");
                return false;
            }

            if (line.Trim().Length == 0)
            {
                return true;
            }

            Result<TicketLine> parsed = TicketLineParser.Parse(screeningId, line);
            if (!parsed.IsSuccess)
            {
                _output.WriteLine($"Помилка вводу: {parsed.Error}");
                continue;
            }

            TryAdd(order, parsed.Value!);
        }
    }

    // введення з файлу чи конвеєра не видно на екрані, тому за потреби повторюємо його
    private string? ReadLine()
    {
        string? line = _input.ReadLine();
        if (_echoInput && line is not null)
        {
            _output.WriteLine(line);
        }

        return line;
    }

    private void TryAdd(TicketOrder order, TicketLine ticket)
    {
        try
        {
            _service.AddLine(order.Id, ticket);
            _output.WriteLine($"Додано місце {ticket.Row}/{ticket.Seat}, {ticket.Price:0.00} грн");
        }
        catch (DomainRuleException ex)
        {
            // порушене правило — помилка користувача: пояснюємо й просимо інше місце
            _output.WriteLine($"Не додано: {ex.Message}");
        }
    }

    private void ReadPayment(TicketOrder order)
    {
        while (true)
        {
            _output.Write("> ");
            string? line = ReadLine();
            if (line is null)
            {
                _output.WriteLine("Введення завершено, замовлення лишилося неоплаченим.");
                return;
            }

            Result<decimal> amount = AmountParser.Parse(line);
            if (!amount.IsSuccess)
            {
                _output.WriteLine($"Помилка вводу: {amount.Error}");
                continue;
            }

            try
            {
                _service.Pay(order.Id, amount.Value);
                _output.WriteLine($"Оплачено. Стан замовлення: {order.Status}");
                return;
            }
            catch (DomainRuleException ex)
            {
                _output.WriteLine($"Оплату відхилено: {ex.Message}");
            }
        }
    }
}
