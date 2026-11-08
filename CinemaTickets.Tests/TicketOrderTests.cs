using CinemaTickets.Core.Domain;

namespace CinemaTickets.Tests;

public class TicketOrderTests
{
    private static readonly DateTimeOffset _startsAt = new(2026, 3, 10, 19, 0, 0, TimeSpan.Zero);

    [Fact]
    public void AddLine_SameSeatTwice_Throws()
    {
        var order = new TicketOrder(1, 100, DateTimeOffset.Now);
        order.AddLine(new TicketLine(screeningId: 1, row: 5, seat: 7, price: 250m));

        Assert.Throws<InvalidOperationException>(
            () => order.AddLine(new TicketLine(screeningId: 1, row: 5, seat: 7, price: 300m)));
    }

    [Fact]
    public void MarkUsed_PaidOrder_BecomesUsed()
    {
        TicketOrder order = PaidOrder();

        order.MarkUsed();

        Assert.Equal(TicketStatus.Used, order.Status);
    }

    [Fact]
    public void MarkUsed_NewOrder_Throws()
    {
        var order = new TicketOrder(1, 100, DateTimeOffset.Now);

        Assert.Throws<InvalidOperationException>(order.MarkUsed);
    }

    [Fact]
    public void Refund_TwoHoursBeforeStart_BecomesRefunded()
    {
        TicketOrder order = PaidOrder();

        order.Refund(_startsAt.AddHours(-2), _startsAt);

        Assert.Equal(TicketStatus.Refunded, order.Status);
    }

    [Fact]
    public void Refund_HalfHourBeforeStart_Throws()
    {
        TicketOrder order = PaidOrder();

        Assert.Throws<InvalidOperationException>(
            () => order.Refund(_startsAt.AddMinutes(-30), _startsAt));
    }

    [Fact]
    public void ArchivedTicketOrder_PaidOrder_ExposesItsData()
    {
        TicketOrder order = PaidOrder();

        var archived = new ArchivedTicketOrder(order, new DateOnly(2026, 3, 11));

        Assert.Equal(1, archived.Id);
        Assert.Equal(TicketStatus.Paid, archived.Status);
        Assert.Single(archived.Lines);
        Assert.Equal(new DateOnly(2026, 3, 11), archived.ArchivedAt);
    }

    private static TicketOrder PaidOrder()
    {
        var order = new TicketOrder(1, 100, _startsAt.AddDays(-1));
        order.AddLine(new TicketLine(screeningId: 1, row: 5, seat: 7, price: 250m));
        order.Pay();
        return order;
    }
}
