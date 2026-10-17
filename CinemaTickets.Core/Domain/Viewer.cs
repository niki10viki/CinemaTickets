namespace CinemaTickets.Core.Domain;

/// <summary>Глядач кінотеатру.</summary>
public sealed class Viewer
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public bool HasLoyaltyCard { get; init; }
}