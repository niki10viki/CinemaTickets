using System.Reflection;

namespace CinemaTickets.Core;

/// <summary>
/// Повертає версію збірки, задану в Directory.Build.props.
/// </summary>
public static class VersionInfo
{
    /// <summary>
    /// Поточна версія без суфікса з хешем коміту.
    /// </summary>
    /// <returns>Версія у форматі МАЖОР.МІНОР.ПАТЧ.</returns>
    public static string Current()
    {
        var assembly = typeof(VersionInfo).Assembly;
        var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        var raw = attribute?.InformationalVersion ?? "0.0.0";
        var plus = raw.IndexOf('+', StringComparison.Ordinal);
        return plus < 0 ? raw : raw[..plus];
    }
}
