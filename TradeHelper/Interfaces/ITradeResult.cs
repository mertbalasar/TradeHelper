using Binance.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TradeHelper.Enums.EnumLibrary;

namespace TradeHelper.Interfaces
{
    public interface ITradeResult
    {
        string Symbol { get; set; }
        decimal PNL { get; set; }
        DateTime TimeStamp { get; set; }
        decimal Price { get; set; }
        decimal FeeUSDT { get; set; }
        PositionSide PositionSide { get; set; }
        OrderSide OrderSide { get; set; }
        decimal QuantityUSDT { get; set; }
        CommissionCategory CommissionCategory { get; set; }
    }
}
