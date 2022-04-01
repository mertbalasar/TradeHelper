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

        public async Task<IProcessResult> OpenOrderAsync(string symbol, decimal costAmount, OrderSide orderSide, int leverage, IOrderType orderType, FuturesMarginType marginType = FuturesMarginType.Isolated, bool reduceOnly = false, bool closeAll = false)
        {
            ProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;

            #region RoundAmount
            IProcessResult roundedAmountResult = await TradeHelpers.FilterAmountByPrecisionAsync(symbol, costAmount * leverage);
            if (roundedAmountResult.Status == ProcessStatus.Fail)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = roundedAmountResult.Message;
                return result;
            }

            decimal reCalculatedAmount = (decimal)roundedAmountResult.Data;
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

                IProcessResult roundedPriceResult = await TradeHelpers.FilterPriceByPrecisionAsync(symbol, limit.LimitPrice);
                if (roundedPriceResult.Status == ProcessStatus.Fail)
                {
                    result.Status = ProcessStatus.Fail;
                    result.Message = roundedPriceResult.Message;
                    return result;
                }

                var positionResult = await client.UsdFuturesApi.Trading.PlaceOrderAsync(symbol, orderSide, futuresOrderType, reCalculatedAmount,
                    reduceOnly: reduceOnly,
                    closePosition: closeAll,
                    price: (decimal)roundedPriceResult.Data,
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

                var positionResult = await client.UsdFuturesApi.Trading.PlaceOrderAsync(symbol, orderSide, futuresOrderType, reCalculatedAmount,
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

                IProcessResult roundedPriceResult = await TradeHelpers.FilterPriceByPrecisionAsync(symbol, trailingStopMarket.ActivationPrice);
                if (roundedPriceResult.Status == ProcessStatus.Fail)
                {
                    result.Status = ProcessStatus.Fail;
                    result.Message = roundedPriceResult.Message;
                    return result;
                }

                var positionResult = await client.UsdFuturesApi.Trading.PlaceOrderAsync(symbol, orderSide, futuresOrderType, reCalculatedAmount,
                    reduceOnly: reduceOnly,
                    closePosition: closeAll,
                    callbackRate: trailingStopMarket.CallbackRate,
                    activationPrice: (decimal)roundedPriceResult.Data,
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

        public async Task<IProcessResult> GetOrderStatus(IOrderResult order)
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

            List<BinanceFuturesOrder> orderList = openOrderResult.Data.ToList().Where((element) => element.Id == order.OrderID).ToList();

            if (orderList.Count > 0)
            {
                result.Data = OrderLocation.OpenOrders;
                return result;
            }
            else
            {
                result.Data = OrderLocation.TradeHistory;
                return result;
            }
        }

        public async Task<IProcessResult> SetTakeProfitAsync(IOrderResult openedOrder, decimal netPrice, WorkingType priceType = WorkingType.Contract)
        {
            ProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;

            IProcessResult orderStatusResult = await GetOrderStatus(openedOrder);
            if (orderStatusResult.Status == ProcessStatus.Fail)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = orderStatusResult.Message;
                return result;
            }

            if ((OrderLocation)orderStatusResult.Data == OrderLocation.OpenOrders)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = "The given order is still open";
                return result;
            }

            OrderSide newSide;
            if (openedOrder.OrderSide == OrderSide.Buy) newSide = OrderSide.Sell;
            else newSide = OrderSide.Buy;

            var orderResult = await client.UsdFuturesApi.Trading.PlaceOrderAsync(
                openedOrder.Symbol,
                newSide,
                FuturesOrderType.TakeProfitMarket,  
                quantity: null,
                reduceOnly: true,
                closePosition: true,
                stopPrice: netPrice,
                workingType: priceType);
            if (!orderResult.Success)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = orderResult.Error.Message;
                return result;
            }

            return result;
        }

        public async Task<IProcessResult> SetStopLossAsync(IOrderResult openedOrder, decimal? netPrice = null, decimal? percentPrice = null)
        {
            ProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;

            IProcessResult orderStatusResult = await GetOrderStatus(openedOrder);
            if (orderStatusResult.Status == ProcessStatus.Fail)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = orderStatusResult.Message;
                return result;
            }

            if ((OrderLocation)orderStatusResult.Data == OrderLocation.OpenOrders)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = "The given order is still open";
                return result;
            }

            futuresOrderType = FuturesOrderType.StopMarket;
            StopMarket stopMarket = (StopMarket)orderType;
            var positionResult = await client.UsdFuturesApi.Trading.PlaceOrderAsync(symbol, side, futuresOrderType, reCalculatedAmount, reduceOnly: reduceOnly,
                stopPrice: stopMarket.StopPrice,
                workingType: stopMarket.StopPriceType);
            if (!positionResult.Success)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = positionResult.Error.Message;
                return result;
            }

            placedOrder = positionResult.Data;

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

            // ORDERLIST EĞER VERİLEN NUMARALI ORDER AÇIKSA COUNT > 0 ŞEKLİNDE BİR LİSTE OLUYOR, YOKSA COUNT = 0 OLUYOR FAKAT HİÇBİR ZAMAN NULL OLMUYOR
            if (orderList.Count > 0)
            {
                return result;
            }
            else
            {
                var positionInfoResult = await client.UsdFuturesApi.Trading.GetUserTradesAsync(order.Symbol);
                if (!positionInfoResult.Success)
                {
                    result.Status = ProcessStatus.Fail;
                    result.Message = positionInfoResult.Error.Message;
                    return result;
                }

                if (openedOrder.OrderType == FuturesOrderType.Limit || openedOrder.OrderType == FuturesOrderType.Market)
                {
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

                    result.Data = positionData;

                    IProcessResult reportResult = ReportProcessor.AddReport(positionData);
                    if (reportResult.Status == ProcessStatus.Fail)
                    {
                        result.Status = ProcessStatus.Fail;
                        result.Message = reportResult.Message;
                        return result;
                    }
                }
                else
                {
                    List<BinanceFuturesUsdtTrade> tradeDetails = positionInfoResult.Data.ToList().Where((element) => element.OrderId == openedOrder.OrderID).ToList();
                    TradeResult tradeData = new TradeResult();

                    if (tradeDetails != null && tradeDetails.Count > 0)
                    {
                        foreach (BinanceFuturesUsdtTrade trade in tradeDetails)
                        {
                            tradeData.Symbol = trade.Symbol;
                            tradeData.ClosePrice = trade.Price;
                            tradeData.CloseTime = trade.Timestamp.AddHours(GMTForGraph);
                            tradeData.PNL += trade.RealizedPnl;
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
                }

                return result;
            }
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

            List<BinanceFuturesUsdtTrade> tradeDetailsForSell = positionInfoResult.Data.ToList().Where((element) => element.OrderId == positionResult.Data.Id).ToList();

            TradeResult tradeData = new TradeResult();
            tradeData.PNL = 0;
            tradeData.FeeUSDT = 0;
            tradeData.Symbol = openedPosition.Symbol;

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

        public async Task<IProcessResult> CancelOrderAsync(IOrderResult openedOrder)
        {
            ProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;

            var closeOrderResult = await client.UsdFuturesApi.Trading.CancelOrderAsync(openedOrder.Symbol, orderId: openedOrder.OrderID);
            if (!closeOrderResult.Success)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = closeOrderResult.Error.Message;
                return result;
            }

            return result;
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
