using System.Text;
using CinemaTickets.Core.Domain;

Console.OutputEncoding = Encoding.UTF8;

var z = new zakaz("A-1001", "Іваненко");
z.Add("STANDARD", 3, 250m);
z.Add("BALCONY", 12, 90m);

Console.WriteLine(z.ProcessData(false));
Console.WriteLine(z.ProcessData(true));
Console.WriteLine(z.Ck());
Console.WriteLine(z.Chg(1));
Console.Write(z.Rep());
