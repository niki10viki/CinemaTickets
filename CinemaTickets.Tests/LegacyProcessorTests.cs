using CinemaTickets.Core.Legacy;
using CinemaTickets.Core.Orders;

namespace CinemaTickets.Tests;

public class LegacyProcessorTests
{
    [Fact]
    public void Process_RegularViewer_Returns1292()
    {
        List<SeatLine> lines = new()
        {
            new SeatLine { SeatCategory = "STANDARD", Quantity = 12, UnitPrice = 100m },
            new SeatLine { SeatCategory = "VIP", Quantity = 1, UnitPrice = 250m },
        };
        decimal total = LegacyProcessor.Process(
            1, "Іваненко", "i@ex.com", true, lines,
            new DateOnly(2026, 3, 10), "", "New", false,
            60m, out string next);
        Assert.Equal(1292.70m, total);
        Assert.Equal("Paid", next);
    }

    [Fact]
    public void Process_SmallOrder_AddsServiceFee()
    {
        List<SeatLine> lines = new()
        {
            new SeatLine { SeatCategory = "STANDARD", Quantity = 2, UnitPrice = 200m },
        };
        decimal total = LegacyProcessor.Process(
            2, "Петренко", "p@ex.com", false, lines,
            new DateOnly(2026, 3, 10), "SALE10", "Paid",
            false, 60m, out string next);
        Assert.Equal(420m, total);
        Assert.Equal("Used", next);
    }
}
