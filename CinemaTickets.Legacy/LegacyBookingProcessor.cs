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
        if (items != null)
        {
            if (items.Count > 0)
            {
                if (state == DocState.New || state == DocState.Paid)
                {
                    if (viewerMail != null &&
                        viewerMail.Contains("@"))
                    {
                        _log.Add("ok " + orderId);
                    }
                    else
                    {
                        return "ERR: mail";
                    }
                }
                else
                {
                    return "ERR: state";
                }
            }
            else
            {
                return "ERR: empty";
            }
        }
        else
        {
            return "ERR: null";
        }

        _tmpSum = 0m;
        for (int i = 0; i < items.Count; i++)
        {
            _tmpSum += items[i].Qty * items[i].Price;
        }

        _tmpDiscount = 0m;
        if (viewerKind == ViewerKind.Vip)
        {
            _tmpDiscount = _tmpSum * PricingRules.VipRate;
            if (_tmpDiscount > PricingRules.MaxDiscount) _tmpDiscount = PricingRules.MaxDiscount;
        }
        else if (viewerKind == ViewerKind.Staff)
        {
            _tmpDiscount = _tmpSum * PricingRules.StaffRate;
            if (_tmpDiscount > PricingRules.MaxDiscount) _tmpDiscount = PricingRules.MaxDiscount;
        }
        else if (viewerDone > PricingRules.LoyalOrders)
        {
            _tmpDiscount = _tmpSum * PricingRules.LoyalRate;
            if (_tmpDiscount > PricingRules.MaxDiscount) _tmpDiscount = PricingRules.MaxDiscount;
        }

        decimal ship = 0m;
        if (_tmpSum - _tmpDiscount < PricingRules.FreeShipFrom) ship = PricingRules.ShipCost;
        decimal total = _tmpSum - _tmpDiscount + ship;

        string txt = "Документ #" + orderId + "\n";
        txt += "Клієнт: " + viewerName + "\n";
        for (int i = 0; i < items.Count; i++)
        {
            txt += items[i].Seat + " x" + items[i].Qty + " = " +
                (items[i].Qty * items[i].Price)
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

    public decimal Preview(ViewerKind viewerKind, int viewerDone,
        List<TicketItem> items)
    {
        decimal s = 0m;
        foreach (var it in items)
        {
            s += it.Qty * it.Price;
        }
        decimal d = 0m;
        if (viewerKind == ViewerKind.Vip)
        {
            d = s * PricingRules.VipRate;
            if (d > PricingRules.MaxDiscount) d = PricingRules.MaxDiscount;
        }
        else if (viewerKind == ViewerKind.Staff)
        {
            d = s * PricingRules.StaffRate;
            if (d > PricingRules.MaxDiscount) d = PricingRules.MaxDiscount;
        }
        else if (viewerDone > PricingRules.LoyalOrders)
        {
            d = s * PricingRules.LoyalRate;
            if (d > PricingRules.MaxDiscount) d = PricingRules.MaxDiscount;
        }
        decimal sh = 0m;
        if (s - d < PricingRules.FreeShipFrom) sh = PricingRules.ShipCost;
        return s - d + sh;
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
