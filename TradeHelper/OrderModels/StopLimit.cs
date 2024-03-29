﻿using Binance.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHelper.Interfaces;

namespace TradeHelper.OrderModels
{
    internal class StopLimit
    {
        public decimal LimitPrice { get; set; }
        public decimal StopPrice { get; set; }
        public WorkingType StopPriceType { get; set; } = WorkingType.Contract;
    }
}
