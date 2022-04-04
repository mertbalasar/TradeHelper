using Binance.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHelper.Controllers;
using TradeHelper.Interfaces;
using TradeHelper.Models;

namespace TradeHelper.AbstractClasses
{
    public abstract class Strategy
    {
        public abstract StrategySettings Settings { get; set; }
        public abstract StrategyTools Tools { get; set; }

        public abstract Task<bool> Initialize();
        public abstract Task<bool> RunAlways();
        public abstract Task<bool> RunTriggered(IProcessResult<List<IKlineResult>> Graphic);
    }
}
