namespace CinemaTickets.Core.Domain;

/// <summary>Кіносеанс: фільм, зала, час початку й базова ціна квитка.</summary>
public sealed class Screening
{
    /// <summary>Ідентифікатор сеансу.</summary>
    public int Id { get; init; }

    /// <summary>Назва фільму.</summary>
    public string MovieTitle { get; init; } = string.Empty;

    /// <summary>Назва зали, у якій проходить сеанс.</summary>
    public string Hall { get; init; } = string.Empty;

    /// <summary>Дата й час початку сеансу.</summary>
    public DateTimeOffset StartsAt { get; init; }

    /// <summary>Базова ціна квитка на сеанс, грн.</summary>
    public decimal BasePrice { get; init; }

    /// <summary>Загальна кількість місць у залі.</summary>
    public int SeatsTotal { get; init; }
}
