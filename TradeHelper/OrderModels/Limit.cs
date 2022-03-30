using Binance.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHelper.Interfaces;

namespace TradeHelper.OrderModels
{
    public class Limit : IOrderParams
    {
        public decimal LimitPrice { get; set; }
        public TimeInForce TimeInForce { get; set; } = TimeInForce.GoodTillCanceled;
    }
}
