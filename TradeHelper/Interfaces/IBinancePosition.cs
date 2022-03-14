using Binance.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHelper.Models;
using static TradeHelper.Enums.EnumLibrary;

namespace TradeHelper.Interfaces
{
    public interface IBinancePosition
    {
        IProcessResult AddCredential(string key, string secret);
        Task<IProcessResult> OpenPositionAsync(string symbol, decimal costAmount, int leverage, PositionType positionType, FuturesMarginType marginType = FuturesMarginType.Isolated);
        Task<IProcessResult> ClosePositionAsync(IPositionResult openedPosition);
        Task<IProcessResult> GetPositionDataAsync(string symbol);
        Task<IProcessResult> GetBalanceAsync();
    }
}
