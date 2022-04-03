using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHelper.Interfaces;

namespace TradeHelper.Models
{
    internal class ReporterDetailsResult : IReporterDetailsResult
    {
        public ITradeResult OpenPosition { get; set; }
        public ITradeResult ClosePosition { get; set; }
    }
}
