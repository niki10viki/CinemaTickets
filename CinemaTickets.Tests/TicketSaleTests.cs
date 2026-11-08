using CinemaTickets.Core.Domain;

namespace CinemaTickets.Tests;

public class TicketSaleTests
{
    // знижки взаємовиключні, після них — оптова знижка за кількістю рядків і ПДВ 20 %
    [Theory]
    [InlineData(1, 1000, true, 1200)]
    [InlineData(1, 1200, true, 1296)]
    [InlineData(1, 6000, false, 6120)]
    [InlineData(1, 6000, true, 6480)]
    [InlineData(11, 100, false, 1200)]
    [InlineData(11, 1, false, 0)]
    public void CalculateTotal_DiscountClasses_MatchesTable(
        int lineCount, double unitPrice, bool isRegular, double expected)
    {
        TicketSale sale = SaleWith(lineCount, (decimal)unitPrice);

        decimal actual = sale.CalculateTotal(isRegular);

        Assert.Equal((decimal)expected, actual);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(3, true)]
    [InlineData(2, false)]
    public void TryChangeStatus_FromNew_FollowsAllowedTransitions(int newStatus, bool expected)
    {
        TicketSale sale = SaleWith(1, 100m);

        bool actual = sale.TryChangeStatus(newStatus);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TryChangeStatus_PaidThenUsed_Succeeds()
    {
        TicketSale sale = SaleWith(1, 100m);
        sale.TryChangeStatus(1);

        bool actual = sale.TryChangeStatus(2);

        Assert.True(actual);
        Assert.Equal(2, sale.Status);
    }

    [Fact]
    public void TryChangeStatus_PaidThenCancelled_IsRejected()
    {
        TicketSale sale = SaleWith(1, 100m);
        sale.TryChangeStatus(1);

        bool actual = sale.TryChangeStatus(3);

        Assert.False(actual);
    }

    [Theory]
    [InlineData("A-1", "Іваненко", 1, true)]
    [InlineData("", "Іваненко", 1, false)]
    [InlineData("A-1", "Ів", 1, false)]
    [InlineData("A-1", "Іваненко", 0, false)]
    public void IsValid_RequiredFields_MatchesTable(
        string id, string customer, int lineCount, bool expected)
    {
        var sale = new TicketSale(id, customer);
        for (int i = 0; i < lineCount; i++)
        {
            sale.AddLine("STANDARD", 1, 100m);
        }

        bool actual = sale.IsValid();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void BuildReport_OneLine_ListsLineAndTotal()
    {
        TicketSale sale = SaleWith(1, 250m);

        string report = sale.BuildReport();

        Assert.Contains("Категорія: STANDARD", report, StringComparison.Ordinal);
        Assert.Contains("Разом: ", report, StringComparison.Ordinal);
    }

    [Fact]
    public void FindById_KnownAndUnknownId_ReturnsSaleOrNull()
    {
        var sales = new List<TicketSale> { SaleWith(1, 100m) };

        TicketSale? found = TicketSale.FindById(sales, "A-1");
        TicketSale? missing = TicketSale.FindById(sales, "Z-9");

        Assert.Same(sales[0], found);
        Assert.Null(missing);
    }

    private static TicketSale SaleWith(int lineCount, decimal unitPrice)
    {
        var sale = new TicketSale("A-1", "Іваненко");
        for (int i = 0; i < lineCount; i++)
        {
            sale.AddLine("STANDARD", 1, unitPrice);
        }

        return sale;
    }
}
