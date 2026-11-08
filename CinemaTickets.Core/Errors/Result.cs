using System.Diagnostics.CodeAnalysis;

namespace CinemaTickets.Core.Errors;

/// <summary>
/// Результат операції з очікуваною помилкою: або значення, або пояснення.
/// </summary>
/// <typeparam name="T">Тип значення в разі успіху.</typeparam>
[SuppressMessage(
    "Design",
    "CA1000:Do not declare static members on generic types",
    Justification = "Фабрики Ok і Fail — задуманий спосіб створення результату.")]
public readonly struct Result<T>
{
    private Result(bool isSuccess, T? value, string error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    /// <summary>Чи вдалася операція.</summary>
    public bool IsSuccess { get; }

    /// <summary>Значення в разі успіху; інакше — значення за замовчуванням.</summary>
    public T? Value { get; }

    /// <summary>Пояснення помилки для користувача; порожнє в разі успіху.</summary>
    public string Error { get; }

    /// <summary>Створює успішний результат.</summary>
    /// <param name="value">Отримане значення.</param>
    /// <returns>Результат зі значенням.</returns>
    public static Result<T> Ok(T value) => new(true, value, string.Empty);

    /// <summary>Створює результат з поясненням помилки.</summary>
    /// <param name="error">Пояснення для користувача.</param>
    /// <returns>Результат без значення.</returns>
    public static Result<T> Fail(string error) => new(false, default, error);
}
