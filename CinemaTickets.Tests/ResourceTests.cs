using CinemaTickets.Core.Abstractions;
using CinemaTickets.Core.Domain;
using CinemaTickets.Core.Services;
using CinemaTickets.Core.Storage;
using Microsoft.Extensions.Logging.Abstractions;

namespace CinemaTickets.Tests;

public class ResourceTests
{
    [Fact]
    public void ExportReport_WhenTotalFails_ReleasesFile()
    {
        string path = Path.Combine(Path.GetTempPath(), $"report-{Guid.NewGuid():N}.txt");
        TicketOrderService service = new(
            new InMemoryTicketOrderRepository(),
            new FailingPricingPolicy(),
            new SilentNotifier(),
            NullLogger<TicketOrderService>.Instance);
        TicketOrder order = new(1, 100, DateTimeOffset.Now);
        order.AddLine(new TicketLine(screeningId: 1, row: 5, seat: 7, price: 250m));

        try
        {
            Assert.Throws<InvalidOperationException>(() => service.ExportReport(path, order));

            // монопольно відкрити файл можна лише тоді, коли попередній потік уже закрито
            using var exclusive = new FileStream(
                path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            Assert.True(exclusive.CanWrite);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private sealed class FailingPricingPolicy : IPricingPolicy
    {
        public decimal PriceOf(TicketLine line) =>
            throw new InvalidOperationException("Тариф недоступний");
    }

    private sealed class SilentNotifier : INotifier
    {
        public void Notify(int viewerId, string message)
        {
        }
    }
}
