using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Objects;
using CryptoExchange.Net.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHelper.Enums;
using TradeHelper.Interfaces;
using TradeHelper.Models;
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

        public async Task<IProcessResult> OpenPositionAsync(string symbol, decimal costAmount, int leverage, PositionType positionType, FuturesMarginType marginType = FuturesMarginType.Isolated)
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

            var positionResult = await client.UsdFuturesApi.Trading.PlaceOrderAsync(symbol, side, FuturesOrderType.Market, costAmount * leverage);
            if (!positionResult.Success)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = positionResult.Error.Message;
                return result;
            }

            var openOrderResult = await client.UsdFuturesApi.Account.GetPositionInformationAsync(symbol: symbol);
            if (!openOrderResult.Success)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = openOrderResult.Error.Message;
                return result;
            }

            PositionResult positionData = new PositionResult();
            var positionDetails = openOrderResult.Data.ToList().FirstOrDefault();

            if (positionDetails != null)
            {
                positionData.Symbol = positionDetails.Symbol;
                positionData.EntryTime = positionDetails.UpdateTime;
                positionData.Side = side;
                positionData.PNL = positionDetails.UnrealizedPnl; // as USDT
                positionData.ROE = 100 * positionDetails.UnrealizedPnl / (positionDetails.Quantity * positionDetails.EntryPrice / positionDetails.Leverage);
                positionData.MarginUSDT = positionDetails.Quantity * positionDetails.EntryPrice / positionDetails.Leverage; // as USDT
                positionData.LiqPrice = positionDetails.LiquidationPrice;
                positionData.Leverage = positionDetails.Leverage;
                positionData.EntryPrice = positionDetails.EntryPrice;
                positionData.MarkPrice = positionDetails.MarkPrice;
                positionData.AddedAmount = positionDetails.Quantity;
            }

            result.Data = positionData;
            IProcessResult reportResult = ReportProcessor.AddReport(positionData);
            if (reportResult.Status == ProcessStatus.Fail)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = reportResult.Message;
                return result;
            }

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
            var tradeDetails = positionInfoResult.Data.ToList().LastOrDefault();

            if (tradeDetails != null)
            {
                tradeData.ClosePrice = tradeDetails.Price;
                tradeData.CloseTime = tradeDetails.Timestamp;
                tradeData.PNL = tradeDetails.RealizedPnl;
                tradeData.Symbol = tradeDetails.Symbol;
                tradeData.FeeUSDT = tradeDetails.Fee;
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

        public async Task<IProcessResult> GetPositionDataAsync(string symbol)
        {
            ProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;

            var openOrderResult = await client.UsdFuturesApi.Account.GetPositionInformationAsync(symbol: symbol);
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
            positionData.EntryTime = positionDetails.UpdateTime;
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
