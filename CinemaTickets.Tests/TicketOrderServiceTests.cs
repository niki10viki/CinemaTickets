using CinemaTickets.Core.Domain;
using CinemaTickets.Core.Errors;
using CinemaTickets.Core.Reporting;
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

    [Fact]
    public void Pay_OrderWithoutTickets_ThrowsOrderEmpty()
    {
        var repository = new FakeTicketOrderRepository();
        repository.Add(new TicketOrder(2, 100, DateTimeOffset.Now));
        var service = NewService(repository, new StubPricingPolicy(1m));

        var ex = Assert.Throws<DomainRuleException>(() => service.Pay(2, 50m));

        Assert.Equal("order.empty", ex.Rule);
        Assert.IsType<InvalidOperationException>(ex.InnerException);
    }

    [Fact]
    public void AddLine_FreeSeat_AddsTicket()
    {
        var (service, order) = ServiceWithOrder();

        service.AddLine(order.Id, Ticket(seat: 9));

        Assert.Equal(2, order.Lines.Count);
    }

    [Fact]
    public void AddLine_SameSeatTwice_ThrowsSeatTaken()
    {
        var (service, order) = ServiceWithOrder();

        var ex = Assert.Throws<DomainRuleException>(() => service.AddLine(order.Id, Ticket(7)));

        Assert.Equal("seat.taken", ex.Rule);
    }

    [Fact]
    public void AddLine_PaidOrder_ThrowsNotEditable()
    {
        var (service, order) = ServiceWithOrder();
        order.Pay();

        var ex = Assert.Throws<DomainRuleException>(() => service.AddLine(order.Id, Ticket(9)));

        Assert.Equal("order.editable", ex.Rule);
    }

    [Fact]
    public void Pay_NotEnoughMoney_ThrowsOrderAmount()
    {
        var (service, order) = ServiceWithOrder();

        var ex = Assert.Throws<DomainRuleException>(() => service.Pay(order.Id, 24.99m));

        Assert.Equal("order.amount", ex.Rule);
    }

    [Fact]
    public void Pay_Overpayment_MarksOrderPaid()
    {
        var (service, order) = ServiceWithOrder();

        service.Pay(order.Id, 100m);

        Assert.Equal(TicketStatus.Paid, order.Status);
    }

    [Fact]
    public void Pay_PaidOrder_ThrowsNotPayable()
    {
        var (service, order) = ServiceWithOrder();
        order.Pay();

        var ex = Assert.Throws<DomainRuleException>(() => service.Pay(order.Id, 25m));

        Assert.Equal("order.payable", ex.Rule);
    }

    [Fact]
    public void Cancel_NewOrder_MarksCancelled()
    {
        var (service, order) = ServiceWithOrder();

        service.Cancel(order.Id, "глядач передумав");

        Assert.Equal(TicketStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Cancel_PaidOrder_ThrowsNotCancellableWithCause()
    {
        var (service, order) = ServiceWithOrder();
        order.Pay();

        var ex = Assert.Throws<DomainRuleException>(() => service.Cancel(order.Id, "пізно"));

        Assert.Equal("order.cancellable", ex.Rule);
        Assert.IsType<InvalidOperationException>(ex.InnerException);
    }

    [Fact]
    public void Load_KnownOrder_ReturnsSameObject()
    {
        var (service, order) = ServiceWithOrder();

        TicketOrder actual = service.Load(order.Id);

        Assert.Same(order, actual);
    }

    [Fact]
    public void SummaryOf_PaidOrder_ReturnsIdStatusAndTotal()
    {
        var (service, order) = ServiceWithOrder();
        order.Pay();

        var summary = service.SummaryOf(order);

        Assert.Equal(new OrderSummary(order.Id, TicketStatus.Paid, 25m), summary);
    }

    [Theory]
    [InlineData(0, "repository")]
    [InlineData(1, "pricing")]
    [InlineData(2, "notifier")]
    [InlineData(3, "log")]
    public void Constructor_NullDependency_ThrowsWithParamName(int nullAt, string expectedParam)
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new TicketOrderService(
            nullAt == 0 ? null! : new FakeTicketOrderRepository(),
            nullAt == 1 ? null! : new StubPricingPolicy(1m),
            nullAt == 2 ? null! : new SpyNotifier(),
            nullAt == 3 ? null! : NullLogger<TicketOrderService>.Instance));

        Assert.Equal(expectedParam, ex.ParamName);
    }

    private static TicketLine Ticket(int seat) =>
        new(screeningId: 1, row: 5, seat: seat, price: 250m);

    // сервіс над фейковим сховищем, у якому вже є нове замовлення з одним квитком на місце 5/7
    private static (TicketOrderService Service, TicketOrder Order) ServiceWithOrder()
    {
        var repository = new FakeTicketOrderRepository();
        var order = new TicketOrder(3, 100, DateTimeOffset.Now);
        order.AddLine(Ticket(7));
        repository.Add(order);
        return (NewService(repository, new StubPricingPolicy(25m)), order);
    }

    private static TicketOrderService NewService(
        FakeTicketOrderRepository repository,
        StubPricingPolicy pricing,
        SpyNotifier? notifier = null) =>
        new(repository, pricing, notifier ?? new SpyNotifier(),
            NullLogger<TicketOrderService>.Instance);
}
