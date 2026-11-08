using System.Diagnostics.CodeAnalysis;

namespace CinemaTickets.Core.Pricing;

/// <summary>
/// Ціна квитка за рядом і часом сеансу: базова ціна з націнками та дитячою знижкою.
/// </summary>
public sealed class TicketPricing
{
    /// <summary>Перший ряд зони комфорту, з якого діє націнка.</summary>
    public const int ComfortRowFrom = 10;

    /// <summary>Націнка за місце в зоні комфорту.</summary>
    public const decimal ComfortRowRate = 0.20m;

    /// <summary>Година початку, з якої сеанс вважається вечірнім.</summary>
    public const int EveningFromHour = 18;

    /// <summary>Націнка за вечірній сеанс.</summary>
    public const decimal EveningRate = 0.10m;

    /// <summary>Знижка на дитячий квиток.</summary>
    public const decimal ChildRate = 0.60m;

    /// <summary>Частка базової ціни, дорожче за яку дитячий квиток не продається.</summary>
    public const decimal ChildMaxShare = 0.50m;

    /// <summary>
    /// Рахує ціну квитка.
    /// </summary>
    /// <param name="basePrice">Базова ціна сеансу, грн.</param>
    /// <param name="row">Ряд, починаючи з 1.</param>
    /// <param name="startHour">Година початку сеансу, від 0 до 23.</param>
    /// <param name="isChild">Чи квиток дитячий.</param>
    /// <returns>Ціна квитка, округлена до копійок.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Від'ємна базова ціна, ряд менший за 1 або година поза межами доби.
    /// </exception>
    [SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification = "Правило ціни передається як об'єкт, щоб його можна було підмінити.")]
    public decimal PriceFor(decimal basePrice, int row, int startHour, bool isChild)
    {
        if (basePrice < 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(basePrice), "Базова ціна не може бути від'ємною.");
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(row);
        ArgumentOutOfRangeException.ThrowIfNegative(startHour);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(startHour, 23);

        decimal rate = 1m;
        if (row >= ComfortRowFrom)
        {
            rate += ComfortRowRate;
        }

        if (startHour >= EveningFromHour)
        {
            rate += EveningRate;
        }

        decimal price = basePrice * rate;
        if (isChild)
        {
            price *= 1m - ChildRate;
            decimal childMax = basePrice * ChildMaxShare;
            if (price > childMax)
            {
                price = childMax;
            }
        }

        return decimal.Round(price, 2);
    }
}
