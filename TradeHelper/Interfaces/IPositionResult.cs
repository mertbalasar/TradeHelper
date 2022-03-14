
using Binance.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHelper.Interfaces
{
    public interface IPositionResult
    {
        string Symbol { get; set; }
        DateTime EntryTime { get; set; }
        OrderSide Side { get; set; }
        int Leverage { get; set; }
        decimal PNL { get; set; }
        decimal ROE { get; set; }
        decimal MarginUSDT { get; set; }
        decimal EntryPrice { get; set; }
        decimal MarkPrice { get; set; }
        decimal LiqPrice { get; set; }
        decimal AddedAmount { get; set; }
    }
}
