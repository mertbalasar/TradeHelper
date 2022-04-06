using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHelper.Enums
{
    public class EnumLibrary
    {
        public enum ProcessStatus { Success, Fail }
        public enum OrderLocation { OpenOrders, OpenPositions, TradeHistory, Unknown }
        public enum CommissionCategory { HighCommission, LowCommission }
    }
}
