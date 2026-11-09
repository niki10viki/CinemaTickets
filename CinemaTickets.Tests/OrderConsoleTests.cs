using CinemaTickets.App;
using CinemaTickets.Core.Domain;
using CinemaTickets.Core.Pricing;
using CinemaTickets.Core.Services;
using CinemaTickets.Core.Storage;
using Microsoft.Extensions.Logging.Abstractions;

namespace CinemaTickets.Tests;

public class OrderConsoleTests
{
    [Fact]
    public void Run_BadLineThenTicketAndPayment_PaysOrder()
    {
        var repository = new InMemoryTicketOrderRepository();
        var service = new TicketOrderService(
            repository,
            new StandardPricingPolicy(),
            new SpyNotifier(),
            NullLogger<TicketOrderService>.Instance);
        var order = new TicketOrder(7, 100, DateTimeOffset.Now);
        repository.Add(order);
        var output = new StringWriter();
        var input = new StringReader("5 сім 250\n5 7 250\n\n300\n");
        var dialog = new OrderConsole(service, input, output, false);

        dialog.Run(order, screeningId: 1);

        Assert.Equal(TicketStatus.Paid, order.Status);
        Assert.Contains(
            "Помилка вводу: Місце — ціле число від 1 до 40",
            output.ToString(),
            StringComparison.Ordinal);
    }

    [Fact]
    public void Run_InputEndsBeforePayment_LeavesOrderNew()
    {
        var repository = new InMemoryTicketOrderRepository();
        var service = new TicketOrderService(
            repository,
            new StandardPricingPolicy(),
            new SpyNotifier(),
            NullLogger<TicketOrderService>.Instance);
        var order = new TicketOrder(8, 100, DateTimeOffset.Now);
        repository.Add(order);

        new OrderConsole(service, new StringReader("5 7 250\n\n"), new StringWriter(), true)
            .Run(order, screeningId: 1);

        Assert.Equal(TicketStatus.New, order.Status);
    }
}
