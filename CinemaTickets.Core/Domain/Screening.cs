namespace CinemaTickets.Core.Domain;

/// <summary>
/// Кіносеанс: незмінний запис розкладу з фільмом, залою, часом початку й базовою ціною.
/// </summary>
public sealed record Screening
{
    /// <summary>
    /// Створює сеанс і перевіряє інваріант довідника.
    /// </summary>
    /// <param name="id">Ідентифікатор сеансу; додатний.</param>
    /// <param name="movieTitle">Назва фільму; не порожня.</param>
    /// <param name="hall">Назва зали; не порожня.</param>
    /// <param name="startsAt">Дата й час початку сеансу.</param>
    /// <param name="basePrice">Базова ціна квитка, грн; додатна.</param>
    /// <param name="seatsTotal">Кількість місць у залі; додатна.</param>
    /// <exception cref="ArgumentException">Порушено інваріант сеансу.</exception>
    public Screening(
        int id,
        string movieTitle,
        string hall,
        DateTimeOffset startsAt,
        decimal basePrice,
        int seatsTotal)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(movieTitle);
        ArgumentException.ThrowIfNullOrWhiteSpace(hall);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(basePrice);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(seatsTotal);

        Id = id;
        MovieTitle = movieTitle;
        Hall = hall;
        StartsAt = startsAt;
        BasePrice = basePrice;
        SeatsTotal = seatsTotal;
    }

    /// <summary>Ідентифікатор сеансу.</summary>
    public int Id { get; }

    /// <summary>Назва фільму.</summary>
    public string MovieTitle { get; }

    /// <summary>Назва зали, у якій проходить сеанс.</summary>
    public string Hall { get; }

    /// <summary>Дата й час початку сеансу.</summary>
    public DateTimeOffset StartsAt { get; }

    /// <summary>Базова ціна квитка на сеанс, грн.</summary>
    public decimal BasePrice { get; }

    /// <summary>Загальна кількість місць у залі.</summary>
    public int SeatsTotal { get; }
}
