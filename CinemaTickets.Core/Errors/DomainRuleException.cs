namespace CinemaTickets.Core.Errors;

/// <summary>
/// Порушено правило предметної області: дію не можна виконати в поточному стані.
/// </summary>
public class DomainRuleException : Exception
{
    /// <summary>
    /// Створює виняток для порушеного правила.
    /// </summary>
    /// <param name="rule">Ідентифікатор правила, наприклад «order.editable».</param>
    /// <param name="message">Пояснення українською з конкретними значеннями.</param>
    public DomainRuleException(string rule, string message)
        : base(message)
    {
        Rule = rule;
    }

    /// <summary>
    /// Створює виняток для порушеного правила зі збереженням первинної причини.
    /// </summary>
    /// <param name="rule">Ідентифікатор правила.</param>
    /// <param name="message">Пояснення українською з конкретними значеннями.</param>
    /// <param name="inner">Виняток, що спричинив порушення.</param>
    public DomainRuleException(string rule, string message, Exception inner)
        : base(message, inner)
    {
        Rule = rule;
    }

    /// <summary>Ідентифікатор порушеного правила.</summary>
    public string Rule { get; }
}
