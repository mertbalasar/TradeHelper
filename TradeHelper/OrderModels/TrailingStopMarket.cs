using Binance.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHelper.Interfaces;

namespace TradeHelper.OrderModels
{
    public class TrailingStopMarket : IOrderParams
    {
        public decimal CallbackRate { get; set; }
        public decimal ActivationPrice { get; set; }
        public WorkingType ActivationPriceType { get; set; } = WorkingType.Contract;
    }
}
