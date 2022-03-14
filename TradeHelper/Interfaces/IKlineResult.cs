using Binance.Net.Interfaces;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHelper.Interfaces
{
    public interface IKlineResult
    {
        string Symbol { get; set; }
        List<IBinanceKline> Klines { get; set; }
        List<Quote> Indicators { get; set; }
    }
}
