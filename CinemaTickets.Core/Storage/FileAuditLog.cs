using System.Globalization;

namespace CinemaTickets.Core.Storage;

/// <summary>
/// Журнал аудиту: дописує у текстовий файл рядки «час;операція;документ».
/// Володіє відкритим файлом, тому реалізує <see cref="IDisposable"/>.
/// </summary>
public sealed class FileAuditLog : IDisposable
{
    private readonly StreamWriter _writer;
    private bool _disposed;

    /// <summary>
    /// Відкриває файл аудиту для дописування.
    /// </summary>
    /// <param name="path">Шлях до файлу аудиту.</param>
    /// <exception cref="IOException">Файл зайнятий іншим процесом.</exception>
    /// <exception cref="UnauthorizedAccessException">Немає прав на запис.</exception>
    public FileAuditLog(string path)
    {
        _writer = new StreamWriter(path, append: true);
    }

    /// <summary>
    /// Дописує рядок аудиту про операцію з документом.
    /// </summary>
    /// <param name="operation">Назва операції, наприклад <c>order.paid</c>.</param>
    /// <param name="documentId">Номер документа.</param>
    /// <exception cref="ObjectDisposedException">Журнал уже закрито.</exception>
    public void Write(string operation, int documentId)
    {
        // запис після закриття — помилка програміста, тому падаємо одразу
        ObjectDisposedException.ThrowIf(_disposed, this);
        _writer.WriteLine(string.Create(
            CultureInfo.InvariantCulture,
            $"{DateTimeOffset.Now:O};{operation};{documentId}"));
    }

    /// <summary>
    /// Дописує буфер і закриває файл; повторний виклик нічого не робить.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _writer.Flush();
        _writer.Dispose();
        _disposed = true;
    }
}
