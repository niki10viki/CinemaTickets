namespace CinemaTickets.Core.Domain;

/// <summary>Кіносеанс: фільм, зала, час початку й базова ціна квитка.</summary>
public sealed class Screening
{
    public int Id { get; init; }

    public string MovieTitle { get; init; } = string.Empty;

    public string Hall { get; init; } = string.Empty;

    public DateTimeOffset StartsAt { get; init; }

    public decimal BasePrice { get; init; }

    public int SeatsTotal { get; init; }
}