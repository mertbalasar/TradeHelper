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
        Task<IProcessResult<IOrderResult>> OpenOrderAsync(string symbol, decimal costAmount, OrderSide orderSide, int leverage, IOrderType orderType, FuturesMarginType marginType = FuturesMarginType.Isolated, bool reduceOnly = false, bool closeAll = false);
        Task<IProcessResult> CancelOrderAsync(IOrderResult openedOrder);
        Task<IProcessResult<OrderLocation>> GetOrderLocationAsync(IOrderResult order);
        Task<IProcessResult<IPositionResult>> GetPositionDataAsync(IOrderResult order);
        Task<IProcessResult<ITradeResult>> GetTradeDataAsync(IOrderResult order);
        Task<IProcessResult> SetTakeProfitAsync(IOrderResult order, decimal netPrice, WorkingType priceType = WorkingType.Contract);
        Task<IProcessResult> SetStopLossAsync(IOrderResult order, decimal netPrice, WorkingType priceType = WorkingType.Contract);
        Task<IProcessResult<IOrderResult>> ClosePositionAsync(IPositionResult openedPosition);
        Task<IProcessResult<decimal>> GetBalanceAsync();
    }
}
