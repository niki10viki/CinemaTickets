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

public class LegacyBookingProcessor
{
    private decimal _tmpSum;
    private decimal _tmpDiscount;
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
        _tmpDiscount = DiscountOf(_tmpSum, viewerKind, viewerDone);
        decimal ship = ShippingOf(_tmpSum - _tmpDiscount);
        decimal total = _tmpSum - _tmpDiscount + ship;

        string txt = "Документ #" + orderId + "\n";
        txt += "Клієнт: " + viewerName + "\n";
        for (int i = 0; i < lines.Count; i++)
        {
            txt += lines[i].Seat + " x" + lines[i].Qty + " = " +
                (lines[i].Qty * lines[i].Price)
                    .ToString("0.00") + " " + currency + "\n";
        }
        txt += "Знижка: " + _tmpDiscount.ToString("0.00") +
            " " + currency + "\n";
        txt += "Доставка: " + ship.ToString("0.00") +
            " " + currency + "\n";
        txt += "Разом: " + total.ToString("0.00") +
            " " + currency + "\n";

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

    private static decimal DiscountOf(
        decimal subtotal, ViewerKind viewerKind, int viewerDone)
    {
        decimal d = 0m;
        if (viewerKind == ViewerKind.Vip)
        {
            d = subtotal * PricingRules.VipRate;
            if (d > PricingRules.MaxDiscount) d = PricingRules.MaxDiscount;
        }
        else if (viewerKind == ViewerKind.Staff)
        {
            d = subtotal * PricingRules.StaffRate;
            if (d > PricingRules.MaxDiscount) d = PricingRules.MaxDiscount;
        }
        else if (viewerDone > PricingRules.LoyalOrders)
        {
            d = subtotal * PricingRules.LoyalRate;
            if (d > PricingRules.MaxDiscount) d = PricingRules.MaxDiscount;
        }
        return d;
    }

    private static decimal ShippingOf(decimal payable)
    {
        decimal ship = 0m;
        if (payable < PricingRules.FreeShipFrom) ship = PricingRules.ShipCost;
        return ship;
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
        var discount = DiscountOf(subtotal, viewerKind, viewerDone);
        var payable = subtotal - discount;
        return payable + ShippingOf(payable);
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
