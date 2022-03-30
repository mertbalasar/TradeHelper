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
        Task<IProcessResult> OpenPositionAsync(string symbol, decimal costAmount, PositionType positionType, IOrderParams orderType, int? leverage = null, bool reduceOnly = false, FuturesMarginType marginType = FuturesMarginType.Isolated);
        Task<IProcessResult> ClosePositionAsync(IPositionResult openedPosition);
        Task<IProcessResult> CancelOrderAsync(IOrderResult openedOrder);
        Task<IProcessResult> GetIsOrderFilledAsync(IOrderResult openedOrder);
        Task<IProcessResult> GetPositionDataAsync(IPositionResult openedPosition);
        Task<IProcessResult> GetBalanceAsync();
    }
}
