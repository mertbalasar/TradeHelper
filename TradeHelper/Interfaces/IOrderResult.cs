using Binance.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHelper.Interfaces
{
    public interface IOrderResult
    {
        string Symbol { get; set; }
        long OrderID { get; set; }
        FuturesOrderType OrderType { get; set; }
    }
}
