﻿using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Objects;
using Binance.Net.Objects.Models.Futures;
using CryptoExchange.Net.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHelper.Enums;
using TradeHelper.Interfaces;
using TradeHelper.Models;
using TradeHelper.OrderModels;
using static TradeHelper.Enums.EnumLibrary;

namespace TradeHelper.Controllers
{
    internal class BinanceProcessor : IBinancePosition
    {
        private BinanceClient client;
        public BinanceProcessor()
        {
            client = new BinanceClient();
        }

        internal int GMTForGraph { get; set; }

        public IProcessResult AddCredential(string key, string secret)
        {
            ProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;

            try
            {
                client.SetApiCredentials(new ApiCredentials(key: key, secret: secret));
            }
            catch (Exception e)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = e.Message;
            }

            return result;
        }

        public async Task<IProcessResult> OpenPositionAsync(string symbol, decimal costAmount, int leverage, PositionType positionType, IOrderParams orderType, FuturesMarginType marginType = FuturesMarginType.Isolated)
        {
            ProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;

            var leverageResult = await client.UsdFuturesApi.Account.ChangeInitialLeverageAsync(symbol, leverage);
            if (!leverageResult.Success)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = leverageResult.Error.Message;
                return result;
            }

            IProcessResult lotSizeResult = await TradeHelpers.GetLotSizeFilterAsync(symbol);
            if (lotSizeResult.Status == ProcessStatus.Fail)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = lotSizeResult.Message;
                return result;
            }

            IProcessResult precisionResult = TradeHelpers.GetPrecisionDecimal((decimal)lotSizeResult.Data);
            int precision = (int)precisionResult.Data;

            if (marginType == FuturesMarginType.Cross)
            {
                var marginTypeResult = await client.UsdFuturesApi.Account.ChangeMarginTypeAsync(symbol, marginType);
                if (!marginTypeResult.Success)
                {
                    result.Status = ProcessStatus.Fail;
                    result.Message = marginTypeResult.Error.Message;
                    return result;
                }
            }

            OrderSide side;
            if (positionType == PositionType.Long) side = OrderSide.Buy;
            else side = OrderSide.Sell;

            decimal reCalculatedAmount = Math.Round(costAmount * leverage, precision);
            FuturesOrderType futuresOrderType = FuturesOrderType.Market;
            BinanceFuturesPlacedOrder placedOrder = null;

            if (orderType.GetType() == typeof(Limit))
            {
                futuresOrderType = FuturesOrderType.Limit;
                Limit limit = (Limit)orderType;
                var positionResult = await client.UsdFuturesApi.Trading.PlaceOrderAsync(symbol, side, futuresOrderType, reCalculatedAmount, 
                    price: limit.LimitPrice, 
                    timeInForce: limit.TimeInForce);
                if (!positionResult.Success)
                {
                    result.Status = ProcessStatus.Fail;
                    result.Message = positionResult.Error.Message;
                    return result;
                }

                placedOrder = positionResult.Data;
            }
            else if (orderType.GetType() == typeof(Market))
            {
                futuresOrderType = FuturesOrderType.Market;
                Market market = (Market)orderType;
                var positionResult = await client.UsdFuturesApi.Trading.PlaceOrderAsync(symbol, side, futuresOrderType, reCalculatedAmount);
                if (!positionResult.Success)
                {
                    result.Status = ProcessStatus.Fail;
                    result.Message = positionResult.Error.Message;
                    return result;
                }

                placedOrder = positionResult.Data;
            }
            else if (orderType.GetType() == typeof(StopLimit))
            {
                futuresOrderType = FuturesOrderType.Stop;
                StopLimit stopLimit = (StopLimit)orderType;
                var positionResult = await client.UsdFuturesApi.Trading.PlaceOrderAsync(symbol, side, futuresOrderType, reCalculatedAmount,
                    price: stopLimit.LimitPrice,
                    stopPrice: stopLimit.StopPrice,
                    workingType: stopLimit.StopPriceType);
                if (!positionResult.Success)
                {
                    result.Status = ProcessStatus.Fail;
                    result.Message = positionResult.Error.Message;
                    return result;
                }

                placedOrder = positionResult.Data;
            }
            else if (orderType.GetType() == typeof(StopMarket))
            {
                futuresOrderType = FuturesOrderType.StopMarket;
                StopMarket stopMarket = (StopMarket)orderType;
                var positionResult = await client.UsdFuturesApi.Trading.PlaceOrderAsync(symbol, side, futuresOrderType, reCalculatedAmount,
                    stopPrice: stopMarket.StopPrice,
                    workingType: stopMarket.StopPriceType);
                if (!positionResult.Success)
                {
                    result.Status = ProcessStatus.Fail;
                    result.Message = positionResult.Error.Message;
                    return result;
                }

                placedOrder = positionResult.Data;
            }
            else if (orderType.GetType() == typeof(TakeProfitLimit))
            {
                futuresOrderType = FuturesOrderType.TakeProfit;
                TakeProfitLimit takeProfitLimit = (TakeProfitLimit)orderType;
                var positionResult = await client.UsdFuturesApi.Trading.PlaceOrderAsync(symbol, side, futuresOrderType, reCalculatedAmount,
                    price: takeProfitLimit.LimitPrice,
                    stopPrice: takeProfitLimit.StopPrice,
                    workingType: takeProfitLimit.StopPriceType);
                if (!positionResult.Success)
                {
                    result.Status = ProcessStatus.Fail;
                    result.Message = positionResult.Error.Message;
                    return result;
                }

                placedOrder = positionResult.Data;
            }
            else if (orderType.GetType() == typeof(TakeProfitMarket))
            {
                futuresOrderType = FuturesOrderType.TakeProfitMarket;
                TakeProfitMarket takeProfitMarket = (TakeProfitMarket)orderType;
                var positionResult = await client.UsdFuturesApi.Trading.PlaceOrderAsync(symbol, side, futuresOrderType, reCalculatedAmount,
                    stopPrice: takeProfitMarket.StopPrice,
                    workingType: takeProfitMarket.StopPriceType);
                if (!positionResult.Success)
                {
                    result.Status = ProcessStatus.Fail;
                    result.Message = positionResult.Error.Message;
                    return result;
                }

                placedOrder = positionResult.Data;
            }
            else if (orderType.GetType() == typeof(TrailingStopMarket))
            {
                futuresOrderType = FuturesOrderType.TrailingStopMarket;
                TrailingStopMarket trailingStopMarket = (TrailingStopMarket)orderType;
                var positionResult = await client.UsdFuturesApi.Trading.PlaceOrderAsync(symbol, side, futuresOrderType, reCalculatedAmount,
                    callbackRate: trailingStopMarket.CallbackRate,
                    activationPrice: trailingStopMarket.ActivationPrice,
                    workingType: trailingStopMarket.ActivationPriceType);
                if (!positionResult.Success)
                {
                    result.Status = ProcessStatus.Fail;
                    result.Message = positionResult.Error.Message;
                    return result;
                }

                placedOrder = positionResult.Data;
            }

            OrderResult orderResult = new OrderResult() { OrderID = placedOrder.Id };
            result.Data = orderResult;

            return result;
        }

        public async Task<IProcessResult> ClosePositionAsync(IPositionResult openedPosition)
        {
            ProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;

            if (openedPosition == null)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = "Not opened any position in yet";
                return result;
            }

            OrderSide side;
            if (openedPosition.Side == OrderSide.Buy) side = OrderSide.Sell;
            else side = OrderSide.Buy;

            var positionResult = await client.UsdFuturesApi.Trading.PlaceOrderAsync(openedPosition.Symbol, side, FuturesOrderType.Market, openedPosition.AddedAmount, reduceOnly: true);
            if (!positionResult.Success)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = positionResult.Error.Message;
                return result;
            }

            var positionInfoResult = await client.UsdFuturesApi.Trading.GetUserTradesAsync(openedPosition.Symbol);
            if (!positionInfoResult.Success)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = positionInfoResult.Error.Message;
                return result;
            }

            TradeResult tradeData = new TradeResult();
            tradeData.PNL = 0;
            tradeData.FeeUSDT = 0;
            tradeData.Symbol = openedPosition.Symbol;

            List<BinanceFuturesUsdtTrade> tradeDetailsForBuy = positionInfoResult.Data.ToList().Where((element) => element.OrderId == openedPosition.ID).ToList();
            List<BinanceFuturesUsdtTrade> tradeDetailsForSell = positionInfoResult.Data.ToList().Where((element) => element.OrderId == positionResult.Data.Id).ToList();

            if (tradeDetailsForSell != null && tradeDetailsForSell.Count > 0)
            {
                foreach (BinanceFuturesUsdtTrade trade in tradeDetailsForSell)
                {
                    tradeData.ClosePrice = trade.Price;
                    tradeData.CloseTime = trade.Timestamp.AddHours(GMTForGraph);
                    tradeData.PNL += trade.RealizedPnl;
                    tradeData.FeeUSDT += trade.Fee;
                }
            }

            if (tradeDetailsForBuy != null && tradeDetailsForBuy.Count > 0)
            {
                foreach (BinanceFuturesUsdtTrade trade in tradeDetailsForBuy)
                {
                    tradeData.FeeUSDT += trade.Fee;
                }
            }

            result.Data = tradeData;
            IProcessResult reportResult = ReportProcessor.AddReport(tradeData);
            if (reportResult.Status == ProcessStatus.Fail)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = reportResult.Message;
                return result;
            }

            return result;
        }

        public async Task<IProcessResult> GetIsOrderFilledAsync(IOrderResult openedOrder)
        {
            ProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;

            var openOrderResult = await client.UsdFuturesApi.Trading.GetOpenOrdersAsync();
            if (!openOrderResult.Success)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = openOrderResult.Error.Message;
                return result;
            }

            List<BinanceFuturesOrder> orderList = openOrderResult.Data.ToList().Where((element) => element.Id == openedOrder.OrderID).ToList();
            if (orderList != null && orderList.Count > 0)
            {
                result.Data = orderList.First().Status;
                return result;
            }
            else
            {
                var positionInfoResult = await client.UsdFuturesApi.Trading.GetUserTradesAsync(openedOrder.Symbol);
                if (!positionInfoResult.Success)
                {
                    result.Status = ProcessStatus.Fail;
                    result.Message = positionInfoResult.Error.Message;
                    return result;
                }

                List<BinanceFuturesUsdtTrade> tradeDetails = positionInfoResult.Data.ToList().Where((element) => element.OrderId == openedOrder.OrderID).ToList();
                PositionResult positionData = new PositionResult();

                if (tradeDetails != null && tradeDetails.Count > 0)
                {
                    foreach (BinanceFuturesUsdtTrade trade in tradeDetails)
                    {
                        positionData.Symbol = trade.Symbol;
                        positionData.EntryTime = trade.Timestamp.AddHours(GMTForGraph);
                        positionData.Side = trade.Side;
                        positionData.EntryPrice = trade.Price;
                        positionData.AddedAmount += trade.Quantity;
                    }
                }

                IProcessResult reportResult = ReportProcessor.AddReport(positionData);
                if (reportResult.Status == ProcessStatus.Fail)
                {
                    result.Status = ProcessStatus.Fail;
                    result.Message = reportResult.Message;
                    return result;
                }

                result.Data = positionData;

                return result;
            }
        }

        public async Task<IProcessResult> GetPositionDataAsync(IPositionResult openedPosition)
        {
            ProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;

            var openOrderResult = await client.UsdFuturesApi.Account.GetPositionInformationAsync(symbol: openedPosition.Symbol);
            if (!openOrderResult.Success)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = openOrderResult.Error.Message;
                return result;
            }

            PositionResult positionData = new PositionResult();
            var positionDetails = openOrderResult.Data.ToList().FirstOrDefault();

            if (positionDetails != null && positionDetails.Quantity == 0)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = "Can not find any open position";
                return result;
            }

            OrderSide side;
            if (positionDetails.PositionSide == PositionSide.Long) side = OrderSide.Buy;
            else side = OrderSide.Sell;

            positionData.Symbol = positionDetails.Symbol;
            positionData.EntryTime = positionDetails.UpdateTime.AddHours(GMTForGraph);
            positionData.Side = side;
            positionData.PNL = positionDetails.UnrealizedPnl; // as USDT
            positionData.ROE = 100 * positionDetails.UnrealizedPnl / (positionDetails.Quantity * positionDetails.EntryPrice / positionDetails.Leverage);
            positionData.MarginUSDT = positionDetails.Quantity * positionDetails.EntryPrice / positionDetails.Leverage; // as USDT
            positionData.LiqPrice = positionDetails.LiquidationPrice;
            positionData.Leverage = positionDetails.Leverage;
            positionData.EntryPrice = positionDetails.EntryPrice;
            positionData.MarkPrice = positionDetails.MarkPrice;
            positionData.AddedAmount = positionDetails.Quantity;

            result.Data = positionData;

            return result;
        }

        public async Task<IProcessResult> GetBalanceAsync()
        {
            ProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;

            var balanceResult = await client.UsdFuturesApi.Account.GetAccountInfoAsync();

            if (!balanceResult.Success)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = balanceResult.Error.Message;
                return result;
            }

            result.Data = balanceResult.Data.Assets.ToList().Where((element) => element.Asset.Equals("USDT")).First().AvailableBalance;

            return result;
        }
    }
}
