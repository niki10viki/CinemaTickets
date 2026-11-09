using CinemaTickets.App.Input;
using CinemaTickets.Core.Domain;
using CinemaTickets.Core.Errors;

namespace CinemaTickets.Tests;

public class TicketLineParserTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("5 7")]
    [InlineData("5 7 250 1")]
    [InlineData("0 7 250")]
    [InlineData("31 7 250")]
    [InlineData("5 -2 250")]
    [InlineData("5 41 250")]
    [InlineData("5 7 -10")]
    [InlineData("5 7 20000")]
    [InlineData("п'ять сім двісті")]
    [InlineData("5 7 250                                        ")]
    public void Parse_InvalidLine_ReturnsFailure(string? raw)
    {
        Result<TicketLine> result = TicketLineParser.Parse(1, raw);

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Error);
    }

    [Fact]
    public void Parse_ValidLine_ReturnsTicket()
    {
        Result<TicketLine> result = TicketLineParser.Parse(1, " 5  7 250 ");

        Assert.True(result.IsSuccess);
        Assert.Equal((5, 7, 250m), (result.Value!.Row, result.Value.Seat, result.Value.Price));
    }
}
