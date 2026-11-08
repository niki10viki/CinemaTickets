using System.Globalization;

namespace CinemaTickets.Legacy.Tests;

public class LegacyCharacterizationTests
{
    public LegacyCharacterizationTests()
    {
        // формат чисел залежить від культури середовища,
        // тому фіксуємо її явно
        CultureInfo.CurrentCulture =
            CultureInfo.InvariantCulture;
    }

    private static List<TicketItem> TwoLines() => new()
    {
        new TicketItem { Seat = "A-1", Qty = 2, Price = 150m },
        new TicketItem { Seat = "B-2", Qty = 1, Price = 400m }
    };

    private static BookingRequest Request(
        List<TicketItem>? items, DocState state, string mail,
        ViewerKind kind, int done) =>
        new(1001,
            new Viewer
            {
                Id = 7,
                Name = "Іван",
                Email = mail,
                Kind = kind,
                DoneCount = done
            },
            items, state, "UAH", false);

    private static Viewer ViewerOf(ViewerKind kind, int done) =>
        new() { Kind = kind, DoneCount = done };

    [Theory]
    [InlineData(null, DocState.New, "a@b.c", "ERR: null")]
    [InlineData("empty", DocState.New, "a@b.c", "ERR: empty")]
    [InlineData("two", DocState.Draft, "a@b.c", "ERR: state")]
    [InlineData("two", DocState.New, "no-mail", "ERR: mail")]
    public void Handle_BadInput_ReturnsErrorCode(
        string? kindOfItems, DocState state, string mail,
        string expected)
    {
        var items = kindOfItems switch
        {
            "two" => TwoLines(),
            "empty" => new List<TicketItem>(),
            _ => null
        };
        var sut = new BookingProcessor();

        var actual = sut.Handle(Request(items, state, mail,
            ViewerKind.Regular, 0));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Handle_VipOrder_ReturnsExactReport()
    {
        var sut = new BookingProcessor();

        var report = sut.Handle(Request(TwoLines(), DocState.New,
            "a@b.c", ViewerKind.Vip, 3));

        var expected =
            "Документ #1001\n" +
            "Клієнт: Іван\n" +
            "A-1 x2 = 300.00 UAH\n" +
            "B-2 x1 = 400.00 UAH\n" +
            "Знижка: 105.00 UAH\n" +
            "Доставка: 60.00 UAH\n" +
            "Разом: 655.00 UAH\n";
        Assert.Equal(expected, report);
    }

    [Theory]
    [InlineData(ViewerKind.Regular, 0, 760)]
    [InlineData(ViewerKind.Vip, 0, 655)]
    [InlineData(ViewerKind.Staff, 0, 550)]
    [InlineData(ViewerKind.Regular, 11, 725)]
    public void Preview_ByKind_ReturnsTotal(ViewerKind kind,
        int done, decimal expected)
    {
        var sut = new BookingProcessor();

        var total = sut.Preview(ViewerOf(kind, done), TwoLines());

        Assert.Equal(expected, total);
    }

    [Fact]
    public void Preview_AfterDiscountExactly1000_FreeShipping()
    {
        var sut = new BookingProcessor();
        var items = new List<TicketItem>
        {
            new TicketItem { Seat = "C-3", Qty = 1, Price = 1000m }
        };

        var total = sut.Preview(ViewerOf(ViewerKind.Regular, 0), items);

        Assert.Equal(1000m, total);
    }

    [Fact]
    public void Preview_VipBigOrder_DiscountCappedAt500()
    {
        var sut = new BookingProcessor();
        var items = new List<TicketItem>
        {
            new TicketItem { Seat = "D-4", Qty = 4, Price = 1000m }
        };

        var total = sut.Preview(ViewerOf(ViewerKind.Vip, 0), items);

        Assert.Equal(3500m, total);
    }

    [Fact]
    public void DescribeClient_Vip_BuildsCaption()
    {
        var c = new Viewer
        {
            Name = "  іван  ",
            Email = "IVAN@MAIL.COM",
            Kind = ViewerKind.Vip,
            DoneCount = 12,
            SinceUtc = new DateTime(2020, 1, 1)
        };
        // метод залежить від системного годинника,
        // тому очікування обчислюємо, а не вписуємо
        var years = DateTime.Now.Year - 2020;
        var expected = "ІВАН [VIP] [ЛОЯЛЬНИЙ] " +
            "<ivan@mail.com> стаж " + years;

        var actual = c.Describe();

        Assert.Equal(expected, actual);
    }
}
