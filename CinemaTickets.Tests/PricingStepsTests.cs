using CinemaTickets.Core.Domain;
using CinemaTickets.Core.Orders;

namespace CinemaTickets.Tests;

public class PricingStepsTests
{
    [Fact]
    public void VolumeDiscountOf_Above5000_Returns10Percent()
    {
        Assert.Equal(600m, OrderTotalCalculator.VolumeDiscountOf(6000m));
    }

    [Fact]
    public void CouponDiscountOf_Minus200Below1000_ReturnsZero()
    {
        Assert.Equal(0m, OrderTotalCalculator.CouponDiscountOf(900m, PricingRules.FixedCoupon));
    }

    [Fact]
    public void NextStatusOf_Paid_ReturnsUsed()
    {
        Assert.Equal(
            TicketStatus.Used,
            OrderTotalCalculator.NextStatusOf(TicketStatus.Paid, 100m));
    }

    [Fact]
    public void Calculate_CancelledOrder_ReturnsZeroAndKeepsStatus()
    {
        TotalRequest request = new(
            OrderId: 3,
            Viewer: new ViewerInfo("Коваль", "k@ex.com", IsRegular: false),
            Lines: new List<SeatLine>
            {
                new() { SeatCategory = "STANDARD", Quantity = 2, UnitPrice = 200m },
            },
            CreatedAt: new DateOnly(2026, 3, 10),
            CouponCode: null,
            Status: TicketStatus.Cancelled,
            ServiceFee: 60m);

        TotalResult result = OrderTotalCalculator.Calculate(request);

        Assert.Equal(0m, result.Total);
        Assert.Equal(TicketStatus.Cancelled, result.Status);
    }
}
