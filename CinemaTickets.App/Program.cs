using System.Text;
using CinemaTickets.App;
using CinemaTickets.Core.Abstractions;
using CinemaTickets.Core.Domain;
using CinemaTickets.Core.Orders;
using CinemaTickets.Core.Pricing;
using CinemaTickets.Core.Reporting;
using CinemaTickets.Core.Services;
using CinemaTickets.Core.Storage;

Console.OutputEncoding = Encoding.UTF8;

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

var ticketService = new TicketOrderService(ticketRepository, PolicyFor("loyalty"), notifier);

var ticketOrder = new TicketOrder(1, 100, DateTimeOffset.Now);
ticketOrder.AddLine(new TicketLine(screeningId: 1, row: 5, seat: 7, price: 250m));
ticketOrder.AddLine(new TicketLine(screeningId: 1, row: 5, seat: 8, price: 250m));
ticketService.Place(ticketOrder);
Console.WriteLine($"Сума: {ticketService.TotalOf(ticketOrder):0.00}");

foreach (string viewerType in new[] { "student", "staff", "guest" })
{
    var pricedService = new TicketOrderService(ticketRepository, PolicyFor(viewerType), notifier);
    Console.WriteLine($"{viewerType}: {pricedService.TotalOf(ticketOrder):0.00}");
}

ticketOrder.Pay();
IEnumerable<OrderSummary> summaries = ticketRepository.GetAll().Select(ticketService.SummaryOf);
Console.Write(TicketOrderCsvReport.BuildCsv(summaries));

var archived = new ArchivedTicketOrder(ticketOrder, DateOnly.FromDateTime(DateTime.Today));
Console.WriteLine($"В архіві: № {archived.Id}, {archived.Status}, квитків {archived.Lines.Count}");

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
