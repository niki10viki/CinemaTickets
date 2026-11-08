using System;
using System.Collections.Generic;

namespace CinemaTickets.Legacy;

public class TicketItem
{
    public string Seat = "";
    public int Qty;
    public decimal Price;
    public string Zone = "";
}

public class BookingProcessor
{
    private decimal _tmpSum;
    private decimal _tmpDiscount;
    private readonly DiscountPolicy _discounts = new();
    private readonly ShippingPolicy _shipping = new();
    private readonly ReportBuilder _report = new();
    private readonly List<string> _log = new();

    public string Handle(BookingRequest req)
    {
        var error = Validate(req);
        if (error != null) return error;
        _log.Add("ok " + req.OrderId);
        var lines = req.Items!;

        _tmpSum = Subtotal(lines);
        _tmpDiscount = _discounts.For(_tmpSum, req.Viewer);
        decimal ship = _shipping.For(_tmpSum - _tmpDiscount);
        decimal total = _tmpSum - _tmpDiscount + ship;

        string txt = _report.Build(req, _tmpDiscount, ship, total);

        if (req.SendMail)
        {
            _log.Add("mail -> " + req.Viewer.Email);
        }
        return txt;
    }

    private static decimal Subtotal(IReadOnlyList<TicketItem> items)
    {
        decimal sum = 0m;
        foreach (var it in items)
        {
            sum += it.Qty * it.Price;
        }
        return sum;
    }

    private static string? Validate(BookingRequest req)
    {
        if (req.Items is null) return "ERR: null";
        if (req.Items.Count == 0) return "ERR: empty";
        if (req.State is not (DocState.New or DocState.Paid))
            return "ERR: state";
        if (!req.Viewer.HasValidEmail) return "ERR: mail";
        return null;
    }

    public decimal Preview(Viewer viewer, IReadOnlyList<TicketItem> items)
    {
        var subtotal = Subtotal(items);
        var discount = _discounts.For(subtotal, viewer);
        var payable = subtotal - discount;
        return payable + _shipping.For(payable);
    }

    public string DescribeClient(Viewer c)
    {
        string s = c.Name.Trim().ToUpper();
        if (c.Kind == ViewerKind.Vip) s += " [VIP]";
        if (c.IsLoyal) s += " [ЛОЯЛЬНИЙ]";
        s += " <" + c.Email.ToLower() + ">";
        int years = DateTime.Now.Year - c.SinceUtc.Year;
        s += " стаж " + years;
        return s;
    }

    public string DumpLog()
    {
        string r = "";
        foreach (var l in _log) r += l + "\n";
        return r;
    }
}
