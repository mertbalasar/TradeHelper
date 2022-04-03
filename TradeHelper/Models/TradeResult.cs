using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHelper.Interfaces;

namespace TradeHelper.Models
{
    internal class TradeResult : ITradeResult
    {
        public string Symbol { get; set; }
        public decimal PNL { get; set; }
        public DateTime TimeStamp { get; set; }
        public decimal Price { get; set; }
        public decimal FeeUSDT { get; set; }
    }
}
