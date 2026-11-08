namespace CinemaTickets.Legacy;

public enum ViewerKind
{
    Regular,
    Vip,
    Staff
}

public sealed class Viewer
{
    public int Id { get; init; }

    public string Name { get; init; } = "";

    public string Email { get; init; } = "";

    public ViewerKind Kind { get; init; }

    public int DoneCount { get; init; }

    public DateTime SinceUtc { get; init; }

    public bool HasValidEmail =>
        Email is not null && Email.Contains("@");

    public bool IsLoyal =>
        DoneCount > PricingRules.LoyalOrders;

    public string Describe()
    {
        var s = Name.Trim().ToUpper();
        if (Kind == ViewerKind.Vip) s += " [VIP]";
        if (IsLoyal) s += " [ЛОЯЛЬНИЙ]";
        s += " <" + Email.ToLower() + ">";
        return s + " стаж " +
            (DateTime.Now.Year - SinceUtc.Year);
    }
}
