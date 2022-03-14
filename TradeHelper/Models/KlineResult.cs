using Binance.Net.Interfaces;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHelper.Interfaces;

namespace TradeHelper.Models
{
    internal class KlineResult : IKlineResult
    {
        public string Symbol { get; set; }
        public List<IBinanceKline> Klines { get; set; }
        public List<Quote> Indicators { get; set; }
    }
}
