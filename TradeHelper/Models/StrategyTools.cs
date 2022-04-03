using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHelper.Interfaces;

namespace TradeHelper.Models
{
    public class StrategyTools
    {
        public IBinancePosition Binance { get; set; }
        public ITestPosition Test { get; set; }
        public INotificationProcessor Notification { get; set; }
        public IReportProcessor Report { get; set; }
    }
}
