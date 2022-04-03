using Binance.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHelper.Interfaces;

namespace TradeHelper.Models
{
    internal class PositionResult : IPositionResult
    {
        public string Symbol { get; set; }
        public DateTime EntryTime { get; set; }
        public PositionSide Side { get; set; }
        public int Leverage { get; set; }
        public decimal PNL { get; set; }
        public decimal ROE { get; set; }    
        public decimal MarginUSDT { get; set; }
        public decimal EntryPrice { get; set; }
        public decimal MarkPrice { get; set; }
        public decimal LiquidationPrice { get; set; }
        public decimal Quantity { get; set; }
    }
}
