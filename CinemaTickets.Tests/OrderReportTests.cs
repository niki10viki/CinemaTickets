using CinemaTickets.Core.Domain;
using CinemaTickets.Core.Orders;
using CinemaTickets.Core.Reporting;

namespace CinemaTickets.Tests;

public class OrderReportTests
{
    [Fact]
    public void TextOf_ContainsTotalLine()
    {
        TotalRequest request = BuildSampleRequest();
        TotalResult result = OrderTotalCalculator.Calculate(request);

        string text = OrderReport.TextOf(request, result);

        Assert.Contains("Разом: 1292.70", text);
    }

    [Fact]
    public void Print_WritesReportToOutput()
    {
        StringWriter writer = new();
        OrderReportPrinter printer = new(writer);

        printer.Print("Разом: 10.00");

        Assert.Contains("10.00", writer.ToString());
    }

    private static TotalRequest BuildSampleRequest() => new(
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
}
