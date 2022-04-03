using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHelper.Interfaces
{
    public interface IReportProcessor
    {
        IProcessResult AddReportForOpenPosition(ITradeResult openedPosition);
        IProcessResult AddReportForClosePosition(ITradeResult closedPosition);
        IProcessResult ResetMembers();
    }
}
