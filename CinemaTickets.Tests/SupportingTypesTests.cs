using CinemaTickets.Core.Domain;
using CinemaTickets.Core.Errors;
using CinemaTickets.Core.Pricing;
using CinemaTickets.Core.Reporting;
using CinemaTickets.Core.Storage;

namespace CinemaTickets.Tests;

public class SupportingTypesTests
{
    [Fact]
    public void Viewer_ValidData_KeepsValues()
    {
        var viewer = new Viewer(1, "Іван", "ivan@mail.com") { HasLoyaltyCard = true };

        Assert.Equal("ivan@mail.com", viewer.Email);
        Assert.True(viewer.HasLoyaltyCard);
    }

    [Fact]
    public void Viewer_EmailWithoutAt_ThrowsWithParamName()
    {
        var ex = Assert.Throws<ArgumentException>(() => new Viewer(1, "Іван", "ivan.mail.com"));

        Assert.Equal("email", ex.ParamName);
    }

    [Fact]
    public void Screening_ValidData_KeepsValues()
    {
        var startsAt = new DateTimeOffset(2026, 3, 10, 19, 0, 0, TimeSpan.Zero);

        var screening = new Screening(1, "Дюна", "Зал 2", startsAt, 200m, 120);

        Assert.Equal("Дюна", screening.MovieTitle);
        Assert.Equal(200m, screening.BasePrice);
        Assert.Equal(120, screening.SeatsTotal);
    }

    [Fact]
    public void Screening_ZeroBasePrice_ThrowsWithParamName()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Screening(1, "Дюна", "Зал 2", DateTimeOffset.Now, 0m, 120));

        Assert.Equal("basePrice", ex.ParamName);
    }

    [Fact]
    public void StandardPricingPolicy_LineWithDiscount_ReturnsAmount()
    {
        var line = new TicketLine(screeningId: 1, row: 5, seat: 7, price: 250m, discount: 50m);

        decimal actual = new StandardPricingPolicy().PriceOf(line);

        Assert.Equal(200m, actual);
    }

    [Fact]
    public void StudentPricingPolicy_FullPriceLine_TakesTenPercentOff()
    {
        var line = new TicketLine(screeningId: 1, row: 5, seat: 7, price: 250m);

        decimal actual = new StudentPricingPolicy().PriceOf(line);

        Assert.Equal(225m, actual);
    }

    [Fact]
    public void BuildCsv_OneSummary_WritesHeaderAndInvariantRow()
    {
        var rows = new[] { new OrderSummary(1, TicketStatus.Paid, 450m) };

        string csv = TicketOrderCsvReport.BuildCsv(rows);

        Assert.StartsWith("id;total;status", csv, StringComparison.Ordinal);
        Assert.Contains("1;450.00;Paid", csv, StringComparison.Ordinal);
    }

    [Fact]
    public void InMemoryRepository_SameIdTwice_Throws()
    {
        var repository = new InMemoryTicketOrderRepository();
        repository.Add(new TicketOrder(1, 100, DateTimeOffset.Now));

        Assert.Throws<InvalidOperationException>(
            () => repository.Add(new TicketOrder(1, 200, DateTimeOffset.Now)));
        Assert.Single(repository.GetAll());
    }

    [Fact]
    public void Result_OkAndFail_CarryValueOrError()
    {
        Result<int> ok = Result<int>.Ok(5);
        Result<int> fail = Result<int>.Fail("Ряд — ціле число більше 0");

        Assert.True(ok.IsSuccess);
        Assert.Equal(5, ok.Value);
        Assert.False(fail.IsSuccess);
        Assert.Equal("Ряд — ціле число більше 0", fail.Error);
    }
}
