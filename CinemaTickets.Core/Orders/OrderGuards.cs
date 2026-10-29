namespace CinemaTickets.Core.Orders;

/// <summary>
/// Перевірки передумов для замовлення квитків: реквізити, рядки замовлення.
/// </summary>
internal static class OrderGuards
{
    /// <summary>
    /// Перевіряє реквізити замовлення та глядача.
    /// </summary>
    /// <param name="orderId">Номер замовлення; має бути додатним.</param>
    /// <param name="viewerName">Ім'я глядача; не може бути порожнім.</param>
    /// <param name="viewerEmail">Пошта глядача; має містити символ @.</param>
    /// <exception cref="ArgumentOutOfRangeException">Номер замовлення не додатний.</exception>
    /// <exception cref="ArgumentException">Ім'я чи пошта порожні або пошта без @.</exception>
    public static void EnsureHeaderValid(int orderId, string viewerName, string viewerEmail)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(orderId);
        ArgumentException.ThrowIfNullOrWhiteSpace(viewerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(viewerEmail);
        if (!viewerEmail.Contains('@'))
        {
            throw new ArgumentException("Пошта має містити символ @.", nameof(viewerEmail));
        }
    }

    /// <summary>
    /// Перевіряє весь запит на розрахунок: реквізити, рядки та сервісний збір.
    /// </summary>
    /// <param name="request">Запит на розрахунок замовлення.</param>
    /// <exception cref="ArgumentNullException">Запит або глядача не передано.</exception>
    /// <exception cref="ArgumentException">Реквізити чи рядки замовлення некоректні.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Сервісний збір від'ємний.</exception>
    public static void EnsureRequestValid(TotalRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Viewer);
        EnsureHeaderValid(request.OrderId, request.Viewer.Name, request.Viewer.Email);
        EnsureLinesValid(request.Lines);
        ArgumentOutOfRangeException.ThrowIfNegative(request.ServiceFee);
    }

    /// <summary>
    /// Перевіряє один рядок замовлення.
    /// </summary>
    /// <param name="line">Рядок із категорією місць, кількістю та ціною.</param>
    /// <exception cref="ArgumentNullException">Рядок відсутній.</exception>
    /// <exception cref="ArgumentException">Категорія місць порожня.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Кількість не додатна або ціна від'ємна.
    /// </exception>
    public static void EnsureLineValid(SeatLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        ArgumentException.ThrowIfNullOrWhiteSpace(line.SeatCategory);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(line.Quantity);
        ArgumentOutOfRangeException.ThrowIfNegative(line.UnitPrice);
    }

    /// <summary>
    /// Перевіряє, що замовлення містить хоча б один рядок і всі рядки коректні.
    /// </summary>
    /// <param name="lines">Рядки замовлення.</param>
    /// <exception cref="ArgumentNullException">Колекцію рядків не передано.</exception>
    /// <exception cref="ArgumentException">Замовлення не містить жодного рядка.</exception>
    public static void EnsureLinesValid(IReadOnlyList<SeatLine> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        if (lines.Count == 0)
        {
            throw new ArgumentException("Замовлення не містить жодного місця.", nameof(lines));
        }

        foreach (SeatLine line in lines)
        {
            EnsureLineValid(line);
        }
    }
}
