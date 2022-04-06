using Binance.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHelper.Interfaces;
using static TradeHelper.Enums.EnumLibrary;

namespace TradeHelper.Models
{
    internal class TradeResult : ITradeResult
    {
        public DateTime TimeStamp { get; set; }
        public string Symbol { get; set; }
        public decimal PNL { get; set; }
        public decimal Price { get; set; }
        public decimal QuantityUSDT { get; set; }
        public decimal FeeUSDT { get; set; }
        public PositionSide PositionSide { get; set; }
        public OrderSide OrderSide { get; set; }
        public CommissionCategory CommissionCategory { get; set; }
    }
}
