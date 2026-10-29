using CinemaTickets.Core.Domain;
using CinemaTickets.Core.Orders;

namespace CinemaTickets.Tests;

public class OrderTotalCalculatorTests
{
    [Fact]
    public void Process_RegularViewer_Returns1292()
    {
        List<SeatLine> lines = new()
        {
            new SeatLine { SeatCategory = "STANDARD", Quantity = 12, UnitPrice = 100m },
            new SeatLine { SeatCategory = "VIP", Quantity = 1, UnitPrice = 250m },
        };
        decimal total = OrderTotalCalculator.Process(
            1, "Іваненко", "i@ex.com", true, lines,
            new DateOnly(2026, 3, 10), "", TicketStatus.New, false,
            60m, out TicketStatus next);
        Assert.Equal(1292.70m, total);
        Assert.Equal(TicketStatus.Paid, next);
    }

    [Fact]
    public void Process_SmallOrder_AddsServiceFee()
    {
        List<SeatLine> lines = new()
        {
            new SeatLine { SeatCategory = "STANDARD", Quantity = 2, UnitPrice = 200m },
        };
        decimal total = OrderTotalCalculator.Process(
            2, "Петренко", "p@ex.com", false, lines,
            new DateOnly(2026, 3, 10), "SALE10", TicketStatus.Paid,
            false, 60m, out TicketStatus next);
        Assert.Equal(420m, total);
        Assert.Equal(TicketStatus.Used, next);
    }
}
