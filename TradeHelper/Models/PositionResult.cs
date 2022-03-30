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
        public long ID { get; set; }
        public string Symbol { get; set; }
        public DateTime EntryTime { get; set; }
        public OrderSide Side { get; set; }
        public int Leverage { get; set; }
        public decimal PNL { get; set; }
        public decimal ROE { get; set; }    
        public decimal MarginUSDT { get; set; }
        public decimal EntryPrice { get; set; }
        public decimal MarkPrice { get; set; }
        public decimal LiqPrice { get; set; }
        public decimal AddedAmount { get; set; }
        public decimal FeeUSDT { get; set; }
    }
}
