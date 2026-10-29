using System;
using System.Collections.Generic;
using System.Linq;

namespace CinemaTickets.Core.Domain;

/// <summary>
/// Продаж квитків на сеанс: рядки з категоріями місць, покупець і стан продажу.
/// </summary>
public class TicketSale
{
    private readonly List<string[]> _lines = new List<string[]>();

    // public string prim;   // примітка, поки не треба

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

    /// <summary>Поточний стан продажу.</summary>
    public int Status { get; private set; } // 0-новий,1-оплач,2-відпр,3-скасов

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
            total = total + quantity * unitPrice;
            lineCount = lineCount + 1;
        }

        // if (sum1 > 500) { sum1 = sum1 - 50; } // стара знижка

        // знижки взаємовиключні: діє лише перша з підходящих
        if (isRegularCustomer == true && total > 1000)
        {
            total = total * 0.9m;
        }
        else if (total > 5000)
        {
            total = total * 0.85m;
        }
        else
        {
            total = total;
        }

        if (lineCount > 10)
        {
            total = total - 100;
        }

        if (total < 0)
        {
            total = 0;
        }

        // ПДВ нараховується на суму вже після всіх знижок
        total = total + total * 0.2m;
        return Math.Round(total, 2);
    }

    /// <summary>
    /// Змінює стан продажу, якщо перехід дозволений правилами предметної області.
    /// </summary>
    /// <param name="newStatus">Цільовий стан продажу.</param>
    /// <returns>true, якщо перехід виконано; false, якщо він заборонений.</returns>
    public bool TryChangeStatus(int newStatus)
    {
        if (Status == 0 && newStatus == 1)
        {
            Status = 1;
            return true;
        }

        if (Status == 1 && newStatus == 2)
        {
            Status = 2;
            return true;
        }

        if (Status == 0 && newStatus == 3)
        {
            Status = 3;
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
            && CustomerName.Length > 2
            && _lines.Count > 0
            && _lines.Count < 100
            && Status >= 0
            && Status <= 3)
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
