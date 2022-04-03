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
        DateTime TimeStamp { get; set; }
        long OrderID { get; set; }
        string Symbol { get; set; }
        decimal Quantity { get; set; }
        FuturesOrderType OrderType { get; set; }
        OrderSide OrderSide { get; set; }
        PositionSide PositionSide { get; set; }
        WorkingType PriceType { get; set; }
        bool ClosePosition { get; set; }
        bool ReduceOnly { get; set; }
        decimal? ActivatePrice { get; set; }       
        TimeInForce TimeInForce { get; set; }
    }
}
