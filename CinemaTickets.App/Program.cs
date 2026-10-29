using System.Text;
using CinemaTickets.Core.Domain;
using CinemaTickets.Core.Orders;

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
Console.WriteLine(result.Total);

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
