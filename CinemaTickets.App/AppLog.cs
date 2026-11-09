using Microsoft.Extensions.Logging;

namespace CinemaTickets.App;

/// <summary>
/// Записи журналу застосунку, оголошені делегатами LoggerMessage.
/// </summary>
internal static partial class AppLog
{
    /// <summary>Порушене правило дійшло до верхнього обробника.</summary>
    /// <param name="logger">Журнал застосунку.</param>
    /// <param name="rule">Ідентифікатор правила.</param>
    /// <param name="message">Пояснення для людини.</param>
    [LoggerMessage(101, LogLevel.Warning, "Правило {Rule}: {Message}")]
    public static partial void RuleViolated(ILogger logger, string rule, string message);

    /// <summary>Непередбачений виняток зупинив застосунок.</summary>
    /// <param name="logger">Журнал застосунку.</param>
    /// <param name="exception">Виняток зі стеком.</param>
    [LoggerMessage(102, LogLevel.Critical, "Непередбачений збій")]
    public static partial void UnexpectedFailure(ILogger logger, Exception exception);

    /// <summary>Файл аудиту зайнятий іншим процесом.</summary>
    /// <param name="logger">Журнал застосунку.</param>
    /// <param name="exception">Виняток введення-виведення.</param>
    /// <param name="path">Шлях до файлу аудиту.</param>
    [LoggerMessage(103, LogLevel.Warning, "Журнал аудиту {Path} зайнятий")]
    public static partial void AuditBusy(ILogger logger, Exception exception, string path);

    /// <summary>Немає прав на запис файлу аудиту.</summary>
    /// <param name="logger">Журнал застосунку.</param>
    /// <param name="exception">Виняток доступу.</param>
    /// <param name="path">Шлях до файлу аудиту.</param>
    [LoggerMessage(104, LogLevel.Warning, "Немає прав на запис журналу аудиту {Path}")]
    public static partial void AuditDenied(ILogger logger, Exception exception, string path);
}
