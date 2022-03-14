using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHelper.AbstractClasses;
using TradeHelper.Controllers;

namespace TradeHelper.Interfaces
{
    internal interface IBindStrategy
    {
        Strategy Strategy { get; set; }
        TriggerProcessor Trigger { get; set; }
    }
}
