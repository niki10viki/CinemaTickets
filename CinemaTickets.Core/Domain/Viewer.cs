namespace CinemaTickets.Core.Domain;

/// <summary>Глядач кінотеатру.</summary>
public sealed class Viewer
{
    /// <summary>Ідентифікатор глядача.</summary>
    public int Id { get; init; }

    /// <summary>Ім'я глядача.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Ознака наявності картки лояльності.</summary>
    public bool HasLoyaltyCard { get; init; }
}
