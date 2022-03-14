using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHelper.AbstractClasses;
using TradeHelper.Controllers;
using TradeHelper.Interfaces;

namespace TradeHelper.Models
{
    internal class BindStrategy : IBindStrategy
    {
        public Strategy Strategy { get; set; }
        public TriggerProcessor Trigger { get; set; }
    }
}
