using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TradeHelper.Enums.EnumLibrary;

namespace TradeHelper.Interfaces
{
    public interface ITestPosition
    {
        Task<IProcessResult> OpenPositionAsync(string symbol, decimal costAmount, int leverage, PositionType positionType);
        Task<IProcessResult> ClosePositionAsync(IPositionResult openedPosition);
        Task<IProcessResult> GetPositionDataAsync(IPositionResult openedPosition);
    }
}
