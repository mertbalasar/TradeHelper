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
        public long OrderID { get; set; }
        public string Symbol { get; set; }
        public decimal Quantity { get; set; }
        public FuturesOrderType OrderType { get; set; }
        public OrderSide OrderSide { get; set; }
        public PositionSide PositionSide { get; set; }
        public WorkingType PriceType { get; set; }
        public bool ClosePosition { get; set; }
        public bool ReduceOnly { get; set; }
        public decimal? ActivatePrice { get; set; }
        public TimeInForce TimeInForce { get; set; }
    }
}
