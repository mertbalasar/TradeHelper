using Binance.Net.Clients;
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

        public async Task<IProcessResult<IOrderResult>> OpenOrderAsync(string symbol, decimal costAmount, OrderSide orderSide, int leverage, IOrderType orderType, FuturesMarginType marginType = FuturesMarginType.Isolated, bool reduceOnly = false, bool closeAll = false)
        {
            OrderProcessResult result = new OrderProcessResult();
            result.Status = ProcessStatus.Success;

            #region RoundAmount
            var roundedAmountResult = await TradeHelpers.FilterAmountByPrecisionAsync(symbol, costAmount * leverage);
            if (roundedAmountResult.Status == ProcessStatus.Fail)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = roundedAmountResult.Message;
                return result;
            }
            #endregion

            #region ChangeMarginType
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
            #endregion

            #region ChangeLeverage
            var leverageResult = await client.UsdFuturesApi.Account.ChangeInitialLeverageAsync(symbol, leverage);
            if (!leverageResult.Success)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = leverageResult.Error.Message;
                return result;
            }
            #endregion

            #region PlaceOrder
            FuturesOrderType futuresOrderType = FuturesOrderType.Market;
            BinanceFuturesPlacedOrder placedOrder = null;

            if (orderType.GetType() == typeof(Limit))
            {
                futuresOrderType = FuturesOrderType.Limit;
                Limit limit = (Limit)orderType;

                var roundedPriceResult = await TradeHelpers.FilterPriceByPrecisionAsync(symbol, limit.LimitPrice);
                if (roundedPriceResult.Status == ProcessStatus.Fail)
                {
                    result.Status = ProcessStatus.Fail;
                    result.Message = roundedPriceResult.Message;
                    return result;
                }

                var positionResult = await client.UsdFuturesApi.Trading.PlaceOrderAsync(symbol, orderSide, futuresOrderType, roundedAmountResult.Data,
                    reduceOnly: reduceOnly,
                    closePosition: closeAll,
                    price: roundedPriceResult.Data,
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

                var positionResult = await client.UsdFuturesApi.Trading.PlaceOrderAsync(symbol, orderSide, futuresOrderType, roundedAmountResult.Data,
                    reduceOnly: reduceOnly,
                    closePosition: closeAll);
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

                var roundedPriceResult = await TradeHelpers.FilterPriceByPrecisionAsync(symbol, trailingStopMarket.ActivationPrice);
                if (roundedPriceResult.Status == ProcessStatus.Fail)
                {
                    result.Status = ProcessStatus.Fail;
                    result.Message = roundedPriceResult.Message;
                    return result;
                }

                var positionResult = await client.UsdFuturesApi.Trading.PlaceOrderAsync(symbol, orderSide, futuresOrderType, roundedAmountResult.Data,
                    reduceOnly: reduceOnly,
                    closePosition: closeAll,
                    callbackRate: trailingStopMarket.CallbackRate,
                    activationPrice: roundedPriceResult.Data,
                    workingType: trailingStopMarket.ActivationPriceType);
                if (!positionResult.Success)
                {
                    result.Status = ProcessStatus.Fail;
                    result.Message = positionResult.Error.Message;
                    return result;
                }

                placedOrder = positionResult.Data;
            }
            #endregion

            OrderResult orderResult = new OrderResult() 
            { 
                TimeStamp = placedOrder.UpdateTime,
                OrderID = placedOrder.Id, 
                Symbol = placedOrder.Symbol, 
                OrderType = placedOrder.Type,
                OrderSide = placedOrder.Side,
                Quantity = placedOrder.Quantity,
                PositionSide = placedOrder.PositionSide,
                ReduceOnly = placedOrder.ReduceOnly,
                ClosePosition = placedOrder.ClosePosition,
                ActivatePrice = placedOrder.ActivatePrice,
                PriceType = placedOrder.WorkingType,
                TimeInForce = placedOrder.TimeInForce
            };
            result.Data = orderResult;

            return result;
        }

        public async Task<IProcessResult> CancelOrderAsync(IOrderResult openedOrder)
        {
            ProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;

            var orderLocationResult = await GetOrderLocationAsync(openedOrder);
            if (orderLocationResult.Status == ProcessStatus.Fail)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = orderLocationResult.Message;
                return result;
            }

            if (orderLocationResult.Data != OrderLocation.OpenOrders)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = "The given order is not located in open orders";
                return result;
            }

            var cancelOrderResult = await client.UsdFuturesApi.Trading.CancelOrderAsync(openedOrder.Symbol, orderId: openedOrder.OrderID);
            if (!cancelOrderResult.Success)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = cancelOrderResult.Error.Message;
                return result;
            }

            return result;
        }

        public async Task<IProcessResult<OrderLocation>> GetOrderLocationAsync(IOrderResult order)
        {
            OrderLocationProcessResult result = new OrderLocationProcessResult();
            result.Status = ProcessStatus.Success;

            var openOrderResult = await client.UsdFuturesApi.Trading.GetOpenOrdersAsync();
            if (!openOrderResult.Success)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = openOrderResult.Error.Message;
                return result;
            }

            List<BinanceFuturesOrder> orderList = openOrderResult.Data.ToList().Where((element) => element.Id == order.OrderID).ToList();

            if (orderList.Count > 0)
            {
                result.Data = OrderLocation.OpenOrders;
                return result;
            }
            else
            {
                var userTradeResult = await client.UsdFuturesApi.Trading.GetUserTradesAsync(order.Symbol);
                if (!userTradeResult.Success)
                {
                    result.Status = ProcessStatus.Fail;
                    result.Message = userTradeResult.Error.Message;
                    return result;
                }

                List<BinanceFuturesUsdtTrade> tradeList = userTradeResult.Data.ToList().Where((element) => element.OrderId == order.OrderID).ToList();

                decimal? totalPNL = null;
                foreach (BinanceFuturesUsdtTrade trade in tradeList)
                {
                    if (totalPNL == null) totalPNL = 0;
                    totalPNL += trade.RealizedPnl;
                }

                if (totalPNL == null)
                {
                    result.Data = OrderLocation.Unknown;
                }
                else if ((decimal)totalPNL == 0)
                {
                    result.Data = OrderLocation.OpenPositions;
                }
                else
                {
                    result.Data = OrderLocation.TradeHistory;
                }

                return result;
            }
        }

        public async Task<IProcessResult<IPositionResult>> GetPositionDataAsync(IOrderResult order)
        {
            PositionProcessResult result = new PositionProcessResult();
            result.Status = ProcessStatus.Success;

            var orderLocationResult = await GetOrderLocationAsync(order);
            if (orderLocationResult.Status == ProcessStatus.Fail)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = orderLocationResult.Message;
                return result;
            }

            if (orderLocationResult.Data != OrderLocation.OpenPositions)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = "The given order is not located in positions";
                return result;
            }

            var positionInformationResult = await client.UsdFuturesApi.Account.GetPositionInformationAsync(symbol: order.Symbol);
            if (!positionInformationResult.Success)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = positionInformationResult.Error.Message;
                return result;
            }

            PositionResult positionData = new PositionResult();
            var positionDetails = positionInformationResult.Data.ToList().FirstOrDefault();

            positionData.Symbol = positionDetails.Symbol;
            positionData.EntryTime = positionDetails.UpdateTime.AddHours(GMTForGraph);
            positionData.Side = positionDetails.PositionSide;
            positionData.PNL = positionDetails.UnrealizedPnl; // as USDT
            positionData.ROE = 100 * positionDetails.UnrealizedPnl / (positionDetails.Quantity * positionDetails.EntryPrice / positionDetails.Leverage);
            positionData.MarginUSDT = positionDetails.Quantity * positionDetails.EntryPrice / positionDetails.Leverage; // as USDT
            positionData.LiquidationPrice = positionDetails.LiquidationPrice;
            positionData.Leverage = positionDetails.Leverage;
            positionData.EntryPrice = positionDetails.EntryPrice;
            positionData.MarkPrice = positionDetails.MarkPrice;
            positionData.Quantity = positionDetails.Quantity;

            result.Data = positionData;

            return result;
        }

        public async Task<IProcessResult<ITradeResult>> GetTradeDataAsync(IOrderResult order)
        {
            TradeProcessResult result = new TradeProcessResult();
            result.Status = ProcessStatus.Success;

            var orderLocationResult = await GetOrderLocationAsync(order);
            if (orderLocationResult.Status == ProcessStatus.Fail)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = orderLocationResult.Message;
                return result;
            }

            if (orderLocationResult.Data != OrderLocation.TradeHistory)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = "The given order is not located in trade history";
                return result;
            }

            var userTradesResult = await client.UsdFuturesApi.Trading.GetUserTradesAsync(order.Symbol);
            if (!userTradesResult.Success)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = userTradesResult.Error.Message;
                return result;
            }

            TradeResult tradeData = new TradeResult();
            List<BinanceFuturesUsdtTrade> tradeDetails = userTradesResult.Data.ToList().Where((element) => element.OrderId == order.OrderID).ToList();

            foreach (BinanceFuturesUsdtTrade trade in tradeDetails)
            {
                tradeData.Symbol = trade.Symbol;
                tradeData.PNL += trade.RealizedPnl;
                tradeData.FeeUSDT += trade.Fee;
                tradeData.TimeStamp = trade.Timestamp;
                tradeData.Price = trade.Price;
            }

            result.Data = tradeData;

            return result;
        }

        public async Task<IProcessResult<IOrderResult>> SetTakeProfitAsync(IOrderResult order, decimal netPrice, WorkingType priceType = WorkingType.Contract)
        {
            OrderProcessResult result = new OrderProcessResult();
            result.Status = ProcessStatus.Success;

            var orderLocationResult = await GetOrderLocationAsync(order);
            if (orderLocationResult.Status == ProcessStatus.Fail)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = orderLocationResult.Message;
                return result;
            }

            if (orderLocationResult.Data != OrderLocation.OpenPositions)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = "The given order is not located in positions";
                return result;
            }

            OrderSide newSide;
            if (order.OrderSide == OrderSide.Buy) newSide = OrderSide.Sell;
            else newSide = OrderSide.Buy;

            var roundedPriceResult = await TradeHelpers.FilterPriceByPrecisionAsync(order.Symbol, netPrice);
            if (roundedPriceResult.Status == ProcessStatus.Fail)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = roundedPriceResult.Message;
                return result;
            }

            var orderResult = await client.UsdFuturesApi.Trading.PlaceOrderAsync(
                order.Symbol,
                newSide,
                FuturesOrderType.TakeProfitMarket,  
                quantity: null,
                reduceOnly: true,
                closePosition: true,
                stopPrice: roundedPriceResult.Data,
                workingType: priceType);
            if (!orderResult.Success)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = orderResult.Error.Message;
                return result;
            }

            OrderResult orderResultData = new OrderResult()
            {
                TimeStamp = orderResult.Data.UpdateTime,
                OrderID = orderResult.Data.Id,
                Symbol = orderResult.Data.Symbol,
                OrderType = orderResult.Data.Type,
                OrderSide = orderResult.Data.Side,
                Quantity = orderResult.Data.Quantity,
                PositionSide = orderResult.Data.PositionSide,
                ReduceOnly = orderResult.Data.ReduceOnly,
                ClosePosition = orderResult.Data.ClosePosition,
                ActivatePrice = orderResult.Data.ActivatePrice,
                PriceType = orderResult.Data.WorkingType,
                TimeInForce = orderResult.Data.TimeInForce
            };
            result.Data = orderResultData;

            return result;
        }

        public async Task<IProcessResult<IOrderResult>> SetStopLossAsync(IOrderResult order, decimal netPrice, WorkingType priceType = WorkingType.Contract)
        {
            OrderProcessResult result = new OrderProcessResult();
            result.Status = ProcessStatus.Success;

            var orderLocationResult = await GetOrderLocationAsync(order);
            if (orderLocationResult.Status == ProcessStatus.Fail)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = orderLocationResult.Message;
                return result;
            }

            if (orderLocationResult.Data != OrderLocation.OpenPositions)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = "The given order is not located in positions";
                return result;
            }

            OrderSide newSide;
            if (order.OrderSide == OrderSide.Buy) newSide = OrderSide.Sell;
            else newSide = OrderSide.Buy;

            var roundedPriceResult = await TradeHelpers.FilterPriceByPrecisionAsync(order.Symbol, netPrice);
            if (roundedPriceResult.Status == ProcessStatus.Fail)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = roundedPriceResult.Message;
                return result;
            }

            var orderResult = await client.UsdFuturesApi.Trading.PlaceOrderAsync(
                order.Symbol,
                newSide,
                FuturesOrderType.StopMarket,
                quantity: null,
                reduceOnly: true,
                closePosition: true,
                stopPrice: roundedPriceResult.Data,
                workingType: priceType);
            if (!orderResult.Success)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = orderResult.Error.Message;
                return result;
            }

            OrderResult orderResultData = new OrderResult()
            {
                TimeStamp = orderResult.Data.UpdateTime,
                OrderID = orderResult.Data.Id,
                Symbol = orderResult.Data.Symbol,
                OrderType = orderResult.Data.Type,
                OrderSide = orderResult.Data.Side,
                Quantity = orderResult.Data.Quantity,
                PositionSide = orderResult.Data.PositionSide,
                ReduceOnly = orderResult.Data.ReduceOnly,
                ClosePosition = orderResult.Data.ClosePosition,
                ActivatePrice = orderResult.Data.ActivatePrice,
                PriceType = orderResult.Data.WorkingType,
                TimeInForce = orderResult.Data.TimeInForce
            };
            result.Data = orderResultData;

            return result;
        }

        public async Task<IProcessResult<IOrderResult>> ClosePositionAsync(IPositionResult openedPosition)
        {
            OrderProcessResult result = new OrderProcessResult();
            result.Status = ProcessStatus.Success;

            if (openedPosition == null)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = "The given position is null";
                return result;
            }

            OrderSide side;
            if (openedPosition.Side == PositionSide.Long) side = OrderSide.Sell;
            else side = OrderSide.Buy;

            var positionResult = await client.UsdFuturesApi.Trading.PlaceOrderAsync(openedPosition.Symbol, side, FuturesOrderType.Market, openedPosition.Quantity, reduceOnly: true);
            if (!positionResult.Success)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = positionResult.Error.Message;
                return result;
            }

            OrderResult orderResult = new OrderResult()
            {
                TimeStamp = positionResult.Data.UpdateTime,
                OrderID = positionResult.Data.Id,
                Symbol = positionResult.Data.Symbol,
                OrderType = positionResult.Data.Type,
                OrderSide = positionResult.Data.Side,
                Quantity = positionResult.Data.Quantity,
                PositionSide = positionResult.Data.PositionSide,
                ReduceOnly = positionResult.Data.ReduceOnly,
                ClosePosition = positionResult.Data.ClosePosition,
                ActivatePrice = positionResult.Data.ActivatePrice,
                PriceType = positionResult.Data.WorkingType,
                TimeInForce = positionResult.Data.TimeInForce
            };
            result.Data = orderResult;

            return result;
        }

        public async Task<IProcessResult<decimal>> GetBalanceAsync()
        {
            DecimalProcessResult result = new DecimalProcessResult();
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
