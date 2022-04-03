using Binance.Net.Enums;
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
        Task<IProcessResult> OpenPositionAsync(string symbol, decimal costAmount, int leverage, PositionSide positionSide);
        Task<IProcessResult> ClosePositionAsync(ITradeResult openedPosition);
        Task<IProcessResult> GetPositionDataAsync(ITradeResult openedPosition);
    }
}
