namespace CinemaTickets.Core.Domain;

/// <summary>
/// Продаж квитків на сеанс: рядки з категоріями місць, покупець і стан продажу.
/// </summary>
public class TicketSale
{
    /// <summary>Ставка податку на додану вартість.</summary>
    private const decimal VatRate = 0.2m;

    /// <summary>Поріг суми для знижки постійному покупцю.</summary>
    private const decimal RegularDiscountThreshold = 1000m;

    /// <summary>Частка знижки постійному покупцю.</summary>
    private const decimal RegularDiscountRate = 0.10m;

    /// <summary>Поріг суми для знижки на великий продаж.</summary>
    private const decimal LargeOrderThreshold = 5000m;

    /// <summary>Частка знижки на великий продаж.</summary>
    private const decimal LargeOrderDiscountRate = 0.15m;

    /// <summary>Кількість рядків, після якої діє гуртова знижка.</summary>
    private const int BulkLineCount = 10;

    /// <summary>Сума гуртової знижки, грн.</summary>
    private const decimal BulkDiscountAmount = 100m;

    /// <summary>Межа кількості рядків у продажу (не включно).</summary>
    private const int MaxLineCount = 100;

    /// <summary>Найкоротше допустиме ім'я покупця.</summary>
    private const int MinCustomerNameLength = 3;

    /// <summary>Стан «новий»: продаж створено, але не оплачено.</summary>
    private const int StatusNew = 0;

    /// <summary>Стан «оплачений».</summary>
    private const int StatusPaid = 1;

    /// <summary>Стан «використаний»: квитки погашено на сеансі.</summary>
    private const int StatusUsed = 2;

    /// <summary>Стан «скасований».</summary>
    private const int StatusCancelled = 3;

    private readonly List<string[]> _lines = new List<string[]>();

    /// <summary>
    /// Створює новий продаж квитків для покупця.
    /// </summary>
    /// <param name="id">Ідентифікатор продажу.</param>
    /// <param name="customerName">Ім'я покупця.</param>
    public TicketSale(string id, string customerName)
    {
        Id = id;
        CustomerName = customerName;
        CreatedAt = DateTime.Now;
    }

    /// <summary>Ідентифікатор продажу.</summary>
    public string Id { get; private set; }

    /// <summary>Ім'я покупця квитків.</summary>
    public string CustomerName { get; private set; }

    /// <summary>Поточний стан продажу: одна з констант StatusNew … StatusCancelled.</summary>
    public int Status { get; private set; }

    /// <summary>Дата й час створення продажу.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Шукає продаж за ідентифікатором у переданому списку.
    /// </summary>
    /// <param name="sales">Список продажів, у якому виконується пошук.</param>
    /// <param name="saleId">Ідентифікатор шуканого продажу.</param>
    /// <returns>Знайдений продаж або null, якщо продажу з таким ідентифікатором немає.</returns>
    public static TicketSale FindById(List<TicketSale> sales, string saleId)
    {
        for (int i = 0; i < sales.Count; i++)
        {
            if (sales[i].Id == saleId)
            {
                return sales[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Додає до продажу рядок із квитками однієї категорії місць.
    /// </summary>
    /// <param name="seatCategory">Категорія місць у залі.</param>
    /// <param name="quantity">Кількість квитків.</param>
    /// <param name="unitPrice">Ціна одного квитка, грн.</param>
    public void AddLine(string seatCategory, int quantity, decimal unitPrice)
    {
        string[] line = new string[3];
        line[0] = seatCategory;
        line[1] = quantity.ToString();
        line[2] = unitPrice.ToString();
        _lines.Add(line);
    }

    /// <summary>
    /// Обчислює суму до сплати за квитки з урахуванням знижок і податку на додану вартість.
    /// </summary>
    /// <param name="isRegularCustomer">Ознака постійного покупця.</param>
    /// <returns>Сума до сплати, заокруглена до двох знаків.</returns>
    public decimal CalculateTotal(bool isRegularCustomer)
    {
        decimal total = 0;
        int lineCount = 0;
        for (int i = 0; i < _lines.Count; i++)
        {
            int quantity = int.Parse(_lines[i][1]);
            decimal unitPrice = decimal.Parse(_lines[i][2]);
            total += quantity * unitPrice;
            lineCount++;
        }

        // знижки взаємовиключні: діє лише перша з підходящих
        if (isRegularCustomer && total > RegularDiscountThreshold)
        {
            total *= 1m - RegularDiscountRate;
        }
        else if (total > LargeOrderThreshold)
        {
            total *= 1m - LargeOrderDiscountRate;
        }

        if (lineCount > BulkLineCount)
        {
            total -= BulkDiscountAmount;
        }

        if (total < 0)
        {
            total = 0;
        }

        // ПДВ нараховується на суму вже після всіх знижок
        total += total * VatRate;
        return Math.Round(total, 2);
    }

    /// <summary>
    /// Змінює стан продажу, якщо перехід дозволений правилами предметної області.
    /// </summary>
    /// <param name="newStatus">Цільовий стан продажу.</param>
    /// <returns>true, якщо перехід виконано; false, якщо він заборонений.</returns>
    public bool TryChangeStatus(int newStatus)
    {
        if (Status == StatusNew && newStatus == StatusPaid)
        {
            Status = StatusPaid;
            return true;
        }

        if (Status == StatusPaid && newStatus == StatusUsed)
        {
            Status = StatusUsed;
            return true;
        }

        if (Status == StatusNew && newStatus == StatusCancelled)
        {
            Status = StatusCancelled;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Перевіряє, чи заповнено продаж достатньо для оформлення.
    /// </summary>
    /// <returns>true, якщо продаж коректний; інакше false.</returns>
    public bool IsValid()
    {
        if (Id != null
            && Id != ""
            && CustomerName != null
            && CustomerName.Length >= MinCustomerNameLength
            && _lines.Count > 0
            && _lines.Count < MaxLineCount
            && Status >= StatusNew
            && Status <= StatusCancelled)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Формує текстовий звіт за рядками продажу з підсумковою сумою.
    /// </summary>
    /// <returns>Багаторядковий звіт; кожен рядок завершується символом нового рядка.</returns>
    public string BuildReport()
    {
        string report = "";
        for (int i = 0; i < _lines.Count; i++)
        {
            report = report + "Категорія: " + _lines[i][0]
                + "; кількість: " + _lines[i][1]
                + "; ціна: " + _lines[i][2]
                + "; сума: " + (int.Parse(_lines[i][1]) * decimal.Parse(_lines[i][2]))
                + "\n";
        }

        report = report + "Разом: " + CalculateTotal(false) + "\n";
        return report;
    }
}
