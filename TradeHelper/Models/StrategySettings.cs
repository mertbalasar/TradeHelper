using Binance.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHelper.Models
{
    public class StrategySettings
    {
        public KlineInterval? RunTriggeredInterval { get; set; }
        public int RunAlwaysDelay { get; set; }
        public string[] Symbols { get; set; }
        public int GMTForGraph { get; set; }
        public string[] MailList { get; set; }
    }
}
