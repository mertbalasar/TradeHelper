using Binance.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHelper.Interfaces;

namespace TradeHelper.Models
{
    internal class OrderResult : IOrderResult
    {
        public string Symbol { get; set; }
        public long OrderID { get; set; }
        public FuturesOrderType OrderType { get; set; }
    }
}
