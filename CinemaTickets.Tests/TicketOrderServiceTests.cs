using CinemaTickets.Core.Domain;
using CinemaTickets.Core.Errors;
using CinemaTickets.Core.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace CinemaTickets.Tests;

public class TicketOrderServiceTests
{
    [Fact]
    public void TotalOf_TwoLines_SumsPricesFromPolicy()
    {
        var order = new TicketOrder(7, 100, DateTimeOffset.Now);
        order.AddLine(new TicketLine(screeningId: 1, row: 5, seat: 7, price: 250m));
        order.AddLine(new TicketLine(screeningId: 1, row: 5, seat: 8, price: 400m));
        var service = NewService(new FakeTicketOrderRepository(), new StubPricingPolicy(25m));

        decimal actual = service.TotalOf(order);

        Assert.Equal(50m, actual);
    }

    [Fact]
    public void Place_NewOrder_StoresOnceAndNotifiesViewer()
    {
        var repository = new FakeTicketOrderRepository();
        var notifier = new SpyNotifier();
        var service = NewService(repository, new StubPricingPolicy(1m), notifier);
        var order = new TicketOrder(1, 100, DateTimeOffset.Now);

        service.Place(order);

        Assert.Same(order, repository.GetById(1));
        Assert.Equal(1, repository.AddCalls);
        Assert.Equal(100, Assert.Single(notifier.NotifiedViewers));
    }

    [Fact]
    public void Pay_UnknownOrder_ThrowsOrderExists()
    {
        var service = NewService(new FakeTicketOrderRepository(), new StubPricingPolicy(1m));

        var ex = Assert.Throws<DomainRuleException>(() => service.Pay(404, 100m));

        Assert.Equal("order.exists", ex.Rule);
    }

    private static TicketOrderService NewService(
        FakeTicketOrderRepository repository,
        StubPricingPolicy pricing,
        SpyNotifier? notifier = null) =>
        new(repository, pricing, notifier ?? new SpyNotifier(),
            NullLogger<TicketOrderService>.Instance);
}
