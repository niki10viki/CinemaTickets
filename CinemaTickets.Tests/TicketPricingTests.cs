using CinemaTickets.Core.Pricing;

namespace CinemaTickets.Tests;

public class TicketPricingTests
{
    [Fact]
    public void PriceFor_NegativeBasePrice_Throws()
    {
        // Arrange
        var pricing = new TicketPricing();

        // Act
        void Act() => pricing.PriceFor(-0.01m, 5, 12, false);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(Act);
    }

    [Fact]
    public void PriceFor_ZeroBasePrice_ReturnsZero()
    {
        var pricing = new TicketPricing();

        decimal actual = pricing.PriceFor(0m, 5, 12, false);

        Assert.Equal(0m, actual);
    }

    [Fact]
    public void PriceFor_OrdinaryRowDaytime_ReturnsBasePrice()
    {
        var pricing = new TicketPricing();

        decimal actual = pricing.PriceFor(200m, 5, 12, false);

        Assert.Equal(200m, actual);
    }

    [Fact]
    public void PriceFor_LastOrdinaryRow_HasNoSurcharge()
    {
        var pricing = new TicketPricing();

        decimal actual = pricing.PriceFor(200m, 9, 12, false);

        Assert.Equal(200m, actual);
    }

    [Fact]
    public void PriceFor_FirstComfortRow_AddsTwentyPercent()
    {
        var pricing = new TicketPricing();

        decimal actual = pricing.PriceFor(200m, 10, 12, false);

        Assert.Equal(240m, actual);
    }

    [Fact]
    public void PriceFor_LastDaytimeHour_HasNoSurcharge()
    {
        var pricing = new TicketPricing();

        decimal actual = pricing.PriceFor(200m, 5, 17, false);

        Assert.Equal(200m, actual);
    }

    [Fact]
    public void PriceFor_FirstEveningHour_AddsTenPercent()
    {
        var pricing = new TicketPricing();

        decimal actual = pricing.PriceFor(200m, 5, 18, false);

        Assert.Equal(220m, actual);
    }

    [Fact]
    public void PriceFor_ChildDaytime_ReturnsFortyPercent()
    {
        var pricing = new TicketPricing();

        decimal actual = pricing.PriceFor(200m, 5, 12, true);

        Assert.Equal(80m, actual);
    }
}
