using CinemaTickets.Core.Domain;
using CinemaTickets.Core.Orders;

namespace CinemaTickets.Tests;

public class OrderTotalCalculatorTests
{
    [Fact]
    public void Calculate_RegularViewer_Returns1292()
    {
        TotalRequest request = new(
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

        TotalResult result = OrderTotalCalculator.Calculate(request);

        Assert.Equal(1292.70m, result.Total);
        Assert.Equal(TicketStatus.Paid, result.Status);
    }

    [Fact]
    public void Calculate_SmallOrder_AddsServiceFee()
    {
        TotalRequest request = new(
            OrderId: 2,
            Viewer: new ViewerInfo("Петренко", "p@ex.com", IsRegular: false),
            Lines: new List<SeatLine>
            {
                new() { SeatCategory = "STANDARD", Quantity = 2, UnitPrice = 200m },
            },
            CreatedAt: new DateOnly(2026, 3, 10),
            CouponCode: "SALE10",
            Status: TicketStatus.Paid,
            ServiceFee: 60m);

        TotalResult result = OrderTotalCalculator.Calculate(request);

        Assert.Equal(420m, result.Total);
        Assert.Equal(TicketStatus.Used, result.Status);
    }
}
