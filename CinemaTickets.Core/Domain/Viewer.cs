namespace CinemaTickets.Core.Domain;

/// <summary>
/// Глядач кінотеатру: незмінний запис довідника учасників.
/// </summary>
public sealed record Viewer
{
    /// <summary>
    /// Створює глядача і перевіряє інваріант довідника.
    /// </summary>
    /// <param name="id">Ідентифікатор глядача; додатний.</param>
    /// <param name="name">Ім'я глядача; не порожнє.</param>
    /// <param name="email">Пошта глядача; містить символ @.</param>
    /// <exception cref="ArgumentOutOfRangeException">Ідентифікатор не додатний.</exception>
    /// <exception cref="ArgumentException">Ім'я порожнє або пошта без @.</exception>
    public Viewer(int id, string name, string email)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        if (!email.Contains('@'))
        {
            throw new ArgumentException("Пошта має містити символ @.", nameof(email));
        }

        Id = id;
        Name = name;
        Email = email;
    }

    /// <summary>Ідентифікатор глядача.</summary>
    public int Id { get; }

    /// <summary>Ім'я глядача.</summary>
    public string Name { get; }

    /// <summary>Пошта глядача для сповіщень.</summary>
    public string Email { get; }

    /// <summary>Ознака картки лояльності; необов'язкова, задається ініціалізатором.</summary>
    public bool HasLoyaltyCard { get; init; }
}
