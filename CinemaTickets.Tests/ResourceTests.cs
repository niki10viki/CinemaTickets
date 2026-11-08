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

    [Fact]
    public void FileAuditLog_ExceptionInsideUsing_ClosesFile()
    {
        string path = Path.Combine(Path.GetTempPath(), $"audit-{Guid.NewGuid():N}.log");
        try
        {
            Assert.Throws<InvalidOperationException>(() => WriteThenFail(path));

            // Dispose дописав буфер і закрив файл: рядок на місці, файл вільний
            string[] lines = File.ReadAllLines(path);
            Assert.Single(lines);
            Assert.EndsWith(";order.paid;7", lines[0], StringComparison.Ordinal);
            using var exclusive = new FileStream(
                path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void FileAuditLog_WriteAfterDispose_Throws()
    {
        string path = Path.Combine(Path.GetTempPath(), $"audit-{Guid.NewGuid():N}.log");
        var audit = new FileAuditLog(path);
        audit.Dispose();
        audit.Dispose();

        try
        {
            Assert.Throws<ObjectDisposedException>(() => audit.Write("order.paid", 7));
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static void WriteThenFail(string path)
    {
        using var audit = new FileAuditLog(path);
        audit.Write("order.paid", 7);
        throw new InvalidOperationException("Збій посеред операції");
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
