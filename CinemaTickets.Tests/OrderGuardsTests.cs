using CinemaTickets.Core.Orders;

namespace CinemaTickets.Tests;

public class OrderGuardsTests
{
    [Fact]
    public void EnsureLinesValid_NoLines_ThrowsArgument()
    {
        Assert.Throws<ArgumentException>(
            () => OrderGuards.EnsureLinesValid(new List<SeatLine>()));
    }

    [Fact]
    public void EnsureLineValid_ZeroQuantity_ThrowsRange()
    {
        SeatLine line = new() { SeatCategory = "STANDARD", Quantity = 0, UnitPrice = 10m };
        Assert.Throws<ArgumentOutOfRangeException>(() => OrderGuards.EnsureLineValid(line));
    }
}
