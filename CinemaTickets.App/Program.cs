using System.Text;
using CinemaTickets.Core.Domain;

Console.OutputEncoding = Encoding.UTF8;

var sale = new TicketSale("A-1001", "Іваненко");
sale.AddLine("STANDARD", 3, 250m);
sale.AddLine("BALCONY", 12, 90m);

Console.WriteLine(sale.CalculateTotal(false));
Console.WriteLine(sale.CalculateTotal(true));
Console.WriteLine(sale.IsValid());
Console.WriteLine(sale.TryChangeStatus(1));
Console.Write(sale.BuildReport());
