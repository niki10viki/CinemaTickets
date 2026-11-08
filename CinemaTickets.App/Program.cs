using System.Text;
using CinemaTickets.App;
using CinemaTickets.Core.Abstractions;
using CinemaTickets.Core.Domain;
using CinemaTickets.Core.Errors;
using CinemaTickets.Core.Orders;
using CinemaTickets.Core.Pricing;
using CinemaTickets.Core.Reporting;
using CinemaTickets.Core.Services;
using CinemaTickets.Core.Storage;
using Microsoft.Extensions.Logging;

Console.OutputEncoding = Encoding.UTF8;
bool dialog = args.Length > 0 && args[0] == "order";

// журнал іде в потік помилок, щоб не змішуватися з діалогом у стандартному виводі
using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddSimpleConsole(options =>
    {
        options.TimestampFormat = "HH:mm:ss ";
        options.IncludeScopes = true;
    });
    builder.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);

    // у діалозі видно всі рівні; демонстрації ЛР2–ЛР4 бачать лише попередження й збої
    builder.SetMinimumLevel(dialog ? LogLevel.Debug : LogLevel.Warning);
});
ILogger log = loggerFactory.CreateLogger("CinemaTickets.App");

// єдине місце, де перехоплюється будь-який виняток: користувач бачить повідомлення,
// а оболонка — код завершення (1 — порушено правило, 2 — непередбачений збій)
try
{
    if (dialog)
    {
        RunOrderDialog(loggerFactory, reportPath: args.Length > 1 ? args[1] : null);
    }
    else
    {
        RunDemos(loggerFactory);
    }

    return 0;
}
catch (DomainRuleException ex)
{
    log.LogWarning("Правило {Rule}: {Message}", ex.Rule, ex.Message);
    Console.Error.WriteLine(ex.Message);
    return 1;
}
catch (Exception ex)
{
    log.LogCritical(ex, "Непередбачений збій");
    Console.Error.WriteLine("Внутрішня помилка, застосунок зупинено");
    return 2;
}

// ЛР5: діалог оформлення замовлення; помилки введення не зупиняють застосунок
static void RunOrderDialog(ILoggerFactory loggerFactory, string? reportPath)
{
    Console.InputEncoding = Encoding.UTF8;
    var repository = new InMemoryTicketOrderRepository();
    var service = new TicketOrderService(
        repository,
        new DiscountPricingPolicy(0.05m),
        new ConsoleNotifier(),
        loggerFactory.CreateLogger<TicketOrderService>());

    var order = new TicketOrder(7, 100, DateTimeOffset.Now);
    repository.Add(order);
    new OrderConsole(service, Console.In, Console.Out, Console.IsInputRedirected)
        .Run(order, screeningId: 1);

    if (reportPath is not null && order.Status == TicketStatus.Paid)
    {
        SaveReport(service, reportPath, order);
    }
}

// межа застосунку для збою середовища: замовлення вже оплачено, тож лише повідомляємо
static void SaveReport(TicketOrderService service, string path, TicketOrder order)
{
    try
    {
        service.ExportReport(path, order);
        Console.WriteLine($"Звіт збережено: {path}");
    }
    catch (IOException ex)
    {
        Console.WriteLine($"Не вдалося зберегти звіт: {ex.Message}");
    }
}

// демонстрації лабораторних робіт № 2–4
static void RunDemos(ILoggerFactory loggerFactory)
{
    var sale = new TicketSale("A-1001", "Іваненко");
    sale.AddLine("STANDARD", 3, 250m);
    sale.AddLine("BALCONY", 12, 90m);

    Console.WriteLine(sale.CalculateTotal(false));
    Console.WriteLine(sale.CalculateTotal(true));
    Console.WriteLine(sale.IsValid());
    Console.WriteLine(sale.TryChangeStatus(1));
    Console.Write(sale.BuildReport());

    TotalRequest request = BuildDemoRequest();
    TotalResult result = OrderTotalCalculator.Calculate(request);
    string report = OrderReport.TextOf(request, result);
    new OrderReportPrinter(Console.Out).Print(report);

    // ЛР4: композиційний корінь — єдине місце, де створюються конкретні реалізації
    var ticketRepository = new InMemoryTicketOrderRepository();
    INotifier notifier = new ConsoleNotifier();
    ILogger<TicketOrderService> serviceLog = loggerFactory.CreateLogger<TicketOrderService>();

    // таблиця відповідності замість ланцюга if: новий тип глядача — новий рядок і новий клас
    var policies = new Dictionary<string, IPricingPolicy>
    {
        ["loyalty"] = new DiscountPricingPolicy(0.05m),
        ["student"] = new StudentPricingPolicy(),
        ["staff"] = new DiscountPricingPolicy(0.30m),
    };

    IPricingPolicy PolicyFor(string viewerType) =>
        policies.TryGetValue(viewerType, out IPricingPolicy? found)
            ? found
            : new StandardPricingPolicy();

    var ticketService =
        new TicketOrderService(ticketRepository, PolicyFor("loyalty"), notifier, serviceLog);

    var ticketOrder = new TicketOrder(1, 100, DateTimeOffset.Now);
    ticketOrder.AddLine(new TicketLine(screeningId: 1, row: 5, seat: 7, price: 250m));
    ticketOrder.AddLine(new TicketLine(screeningId: 1, row: 5, seat: 8, price: 250m));
    ticketService.Place(ticketOrder);
    Console.WriteLine($"Сума: {ticketService.TotalOf(ticketOrder):0.00}");

    foreach (string viewerType in new[] { "student", "staff", "guest" })
    {
        var pricedService =
            new TicketOrderService(ticketRepository, PolicyFor(viewerType), notifier, serviceLog);
        Console.WriteLine($"{viewerType}: {pricedService.TotalOf(ticketOrder):0.00}");
    }

    ticketOrder.Pay();
    IEnumerable<OrderSummary> summaries = ticketRepository.GetAll().Select(ticketService.SummaryOf);
    Console.Write(TicketOrderCsvReport.BuildCsv(summaries));

    var archived = new ArchivedTicketOrder(ticketOrder, DateOnly.FromDateTime(DateTime.Today));
    Console.WriteLine(
        $"В архіві: № {archived.Id}, {archived.Status}, квитків {archived.Lines.Count}");
}

static TotalRequest BuildDemoRequest() => new(
    OrderId: 1,
    Viewer: new ViewerInfo("Іваненко", "i@ex.com", IsRegular: true),
    Lines: new List<SeatLine>
    {
        new() { SeatCategory = "STANDARD", Quantity = 12, UnitPrice = 100m },
        new() { SeatCategory = "VIP", Quantity = 1, UnitPrice = 250m },
    },
    CreatedAt: new DateOnly(2026, 3, 10),
    CouponCode: null,
    Status: TicketStatus.New,
    ServiceFee: 60m);
