using System.Globalization;
using CinemaTickets.Core.Domain;

Screening screening = new()
{
    Id = 101,
    MovieTitle = "Дюна: Частина друга",
    Hall = "Зала 2",
    StartsAt = new DateTimeOffset(2026, 10, 20, 19, 30, 0, TimeSpan.FromHours(3)),
    BasePrice = 180.00m,
    SeatsTotal = 120,
};

TicketOrder order = new()
{
    Id = 1,
    ViewerId = 10,
    CreatedAt = DateTimeOffset.Now,
};

TicketLine first = new()
{
    ScreeningId = screening.Id, Row = 5, Seat = 7, Price = 180.00m,
};

TicketLine second = new()
{
    ScreeningId = screening.Id, Row = 5, Seat = 8, Price = 220.50m, Discount = 20.50m,
};

order.AddLine(first);
order.AddLine(second);

string total = order.Total().ToString("F2", CultureInfo.InvariantCulture);

Console.WriteLine($"Сеанс: {screening.MovieTitle}, {screening.Hall}");
Console.WriteLine($"Замовлення #{order.Id}");
Console.WriteLine($"Стан: {order.Status}");
Console.WriteLine($"Квитків: {order.Lines.Count}");
Console.WriteLine($"Сума: {total}");