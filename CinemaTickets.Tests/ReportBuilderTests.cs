using CinemaTickets.Core;
using CinemaTickets.Core.Reporting;

namespace CinemaTickets.Tests;

public class ReportBuilderTests
{
    [Fact]
    public void Build_TwoRows_PrintsUpperTitleThenRows()
    {
        var builder = new ReportBuilder { Title = "Каса" };
        var rows = new List<string> { "5/7 — 250", "5/8 — 250" };

        string report = builder.Build(rows);

        string nl = Environment.NewLine;
        Assert.Equal($"КАСА{nl}5/7 — 250{nl}5/8 — 250{nl}", report);
    }

    [Fact]
    public void Build_NullRows_Throws()
    {
        var builder = new ReportBuilder();

        Assert.Throws<ArgumentNullException>(() => builder.Build(null!));
    }

    [Fact]
    public void VersionInfo_Current_MatchesDirectoryBuildProps()
    {
        string version = VersionInfo.Current();

        Assert.Equal("0.8.0", version);
    }
}
