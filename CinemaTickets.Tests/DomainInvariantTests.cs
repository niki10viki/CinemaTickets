using CinemaTickets.Core.Abstractions;
using CinemaTickets.Core.Domain;
using CinemaTickets.Core.Pricing;
using CinemaTickets.Core.Services;
using CinemaTickets.Core.Storage;

namespace CinemaTickets.Tests;

public class DomainInvariantTests
{
    [Fact]
    public void TicketLine_WithZeroRow_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new TicketLine(screeningId: 1, row: 0, seat: 7, price: 250m));
    }

    [Fact]
    public void TicketOrder_AfterPay_RejectsNewLines()
    {
        TicketOrder order = NewOrder(1);
        order.AddLine(new TicketLine(screeningId: 1, row: 5, seat: 7, price: 250m));
        order.Pay();

        Assert.Throws<InvalidOperationException>(
            () => order.AddLine(new TicketLine(screeningId: 1, row: 5, seat: 8, price: 250m)));
    }

    [Fact]
    public void TotalOf_WithTenPercentDiscount_Returns450()
    {
        TicketOrderService service = new(
            new InMemoryTicketOrderRepository(),
            new DiscountPricingPolicy(0.10m),
            new SilentNotifier());
        TicketOrder order = NewOrder(2);
        order.AddLine(new TicketLine(screeningId: 1, row: 5, seat: 7, price: 250m));
        order.AddLine(new TicketLine(screeningId: 1, row: 5, seat: 8, price: 250m));

        Assert.Equal(450m, service.TotalOf(order));
    }

    private static TicketOrder NewOrder(int id) =>
        new(id, viewerId: 100, createdAt: new DateTimeOffset(2026, 3, 10, 19, 0, 0, TimeSpan.Zero));

    // сповіщення в тесті розрахунку не потрібні: заглушка без побічних ефектів
    private sealed class SilentNotifier : INotifier
    {
        public void Notify(int viewerId, string message)
        {
        }
    }
}
