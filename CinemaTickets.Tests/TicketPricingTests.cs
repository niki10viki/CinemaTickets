using CinemaTickets.Core.Pricing;

namespace CinemaTickets.Tests;

public class TicketPricingTests
{
    [Theory]
    [InlineData(0, 5, 12, false, 0)]
    [InlineData(200, 5, 12, false, 200)]
    [InlineData(200, 9, 12, false, 200)]
    [InlineData(200, 10, 12, false, 240)]
    [InlineData(200, 5, 17, false, 200)]
    [InlineData(200, 5, 18, false, 220)]
    [InlineData(200, 10, 18, false, 260)]
    [InlineData(200, 5, 12, true, 80)]
    [InlineData(200, 10, 12, true, 96)]
    [InlineData(100, 10, 18, true, 50)]
    public void PriceFor_Boundaries_MatchesTable(
        double basePrice, int row, int startHour, bool isChild, double expected)
    {
        var pricing = new TicketPricing();

        decimal actual = pricing.PriceFor((decimal)basePrice, row, startHour, isChild);

        Assert.Equal((decimal)expected, actual);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-1000)]
    public void PriceFor_NegativeBasePrice_ThrowsOutOfRange(double basePrice)
    {
        var pricing = new TicketPricing();

        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => pricing.PriceFor((decimal)basePrice, 5, 12, false));

        Assert.Equal("basePrice", ex.ParamName);
    }

    [Theory]
    [InlineData(0, 12, "row")]
    [InlineData(5, -1, "startHour")]
    [InlineData(5, 24, "startHour")]
    public void PriceFor_RowOrHourOutOfRange_ThrowsWithParamName(
        int row, int startHour, string expectedParam)
    {
        var pricing = new TicketPricing();

        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => pricing.PriceFor(200m, row, startHour, false));

        Assert.Equal(expectedParam, ex.ParamName);
    }

    [Fact]
    public void PriceFor_ChildComfortEvening_IsCappedAtHalfBase()
    {
        var pricing = new TicketPricing();

        decimal actual = pricing.PriceFor(200m, 10, 18, true);

        Assert.Equal(100m, actual);
    }
}
