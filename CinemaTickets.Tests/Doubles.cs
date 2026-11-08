using CinemaTickets.Core.Abstractions;
using CinemaTickets.Core.Domain;

namespace CinemaTickets.Tests;

// фейк: спрощене, але робоче сховище в пам'яті; читає те, що в нього записали
internal sealed class FakeTicketOrderRepository : ITicketOrderRepository
{
    private readonly Dictionary<int, TicketOrder> _items = new();

    public int AddCalls { get; private set; }

    public void Add(TicketOrder order)
    {
        _items[order.Id] = order;
        AddCalls++;
    }

    public TicketOrder? GetById(int id) =>
        _items.TryGetValue(id, out TicketOrder? found)
            ? found
            : null;

    public IReadOnlyList<TicketOrder> GetAll() => _items.Values.ToList();
}

// стаб: стану немає, на будь-який квиток — наперед задана ціна
internal sealed class StubPricingPolicy : IPricingPolicy
{
    private readonly decimal _price;

    public StubPricingPolicy(decimal price) => _price = price;

    public decimal PriceOf(TicketLine line) => _price;
}

// шпигун: запам'ятовує сповіщення, щоб тест перевірив сам факт виклику
internal sealed class SpyNotifier : INotifier
{
    public List<int> NotifiedViewers { get; } = new();

    public void Notify(int viewerId, string message) => NotifiedViewers.Add(viewerId);
}
