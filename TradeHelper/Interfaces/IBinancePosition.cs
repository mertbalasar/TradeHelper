﻿using Binance.Net.Enums;
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
        Task<IProcessResult<IOrderResult>> OpenOrderAsync(string symbol, OrderSide orderSide, IOrderType orderType, decimal? costAmount = null, int? leverage = null, bool? reduceOnly = null, bool? closeAll = null, FuturesMarginType marginType = FuturesMarginType.Isolated);
        Task<IProcessResult> CancelOrderAsync(IOrderResult openedOrder);
        Task<IProcessResult<OrderLocation>> GetOrderLocationAsync(IOrderResult order);
        Task<IProcessResult<IPositionResult>> GetPositionDataAsync(IOrderResult order);
        Task<IProcessResult<ITradeResult>> GetTradeDataAsync(IOrderResult order);
        Task<IProcessResult<IOrderResult>> SetTakeProfitAsync(IOrderResult order, decimal netPrice, WorkingType priceType = WorkingType.Contract);
        Task<IProcessResult<IOrderResult>> SetStopLossAsync(IOrderResult order, decimal netPrice, WorkingType priceType = WorkingType.Contract);
        Task<IProcessResult<IOrderResult>> ClosePositionAsync(IPositionResult openedPosition);
        Task<IProcessResult<decimal>> GetBalanceAsync();
        IProcessResult<IBinancePosition> GetNewInstance(int? gmtForGraph = null);
    }
}
