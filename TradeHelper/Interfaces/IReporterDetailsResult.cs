﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHelper.Interfaces
{
    internal interface IReporterDetailsResult
    {
        ITradeResult OpenPosition { get; set; }
        ITradeResult ClosePosition { get; set; }
    }
}
