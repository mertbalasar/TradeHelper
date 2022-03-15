using Binance.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHelper.Controllers;
using TradeHelper.Interfaces;

namespace TradeHelper.AbstractClasses
{
    public abstract class Strategy
    {
        public abstract KlineInterval? RunTriggeredInterval { get; set; }
        public abstract int RunAlwaysDelay { get; set; }
        public abstract string[] Symbols { get; set; }
        public abstract int GMTForGraph { get; set; }
        public abstract IBinancePosition Binance { get; set; }
        public abstract ITestPosition Test { get; set; }

        public abstract Task<bool> Initialize();
        public abstract Task<bool> RunAlways();
        public abstract void RunTriggered(IProcessResult Graphic);
    }
}
