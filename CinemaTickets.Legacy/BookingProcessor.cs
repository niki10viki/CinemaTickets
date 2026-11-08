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

    public string Handle(int orderId, int viewerId,
        string viewerName, string? viewerMail,
        ViewerKind viewerKind, int viewerDone,
        List<TicketItem>? items, DocState state,
        string currency, DateTime createdAt, bool sendMail)
    {
        var error = Validate(items, state, viewerMail);
        if (error != null) return error;
        _log.Add("ok " + orderId);
        var lines = items!;

        _tmpSum = Subtotal(lines);
        _tmpDiscount = _discounts.For(_tmpSum, viewerKind, viewerDone);
        decimal ship = _shipping.For(_tmpSum - _tmpDiscount);
        decimal total = _tmpSum - _tmpDiscount + ship;

        string txt = _report.Build(orderId, viewerName, lines,
            currency, _tmpDiscount, ship, total);

        if (sendMail)
        {
            _log.Add("mail -> " + viewerMail);
        }
        return txt;
    }

    private static decimal Subtotal(List<TicketItem> items)
    {
        decimal sum = 0m;
        foreach (var it in items)
        {
            sum += it.Qty * it.Price;
        }
        return sum;
    }

    private static string? Validate(
        List<TicketItem>? items, DocState state, string? viewerMail)
    {
        if (items is null) return "ERR: null";
        if (items.Count == 0) return "ERR: empty";
        if (state is not (DocState.New or DocState.Paid))
            return "ERR: state";
        if (viewerMail is null || !viewerMail.Contains("@"))
            return "ERR: mail";
        return null;
    }

    public decimal Preview(ViewerKind viewerKind, int viewerDone,
        List<TicketItem> items)
    {
        var subtotal = Subtotal(items);
        var discount = _discounts.For(subtotal, viewerKind, viewerDone);
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
