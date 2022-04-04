using Binance.Net.Clients;
using Binance.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHelper.Interfaces;
using TradeHelper.Models;
using static TradeHelper.Enums.EnumLibrary;

namespace TradeHelper.Controllers
{
    internal class TestExchangeProcessor : ITestPosition
    {
        private BinanceClient client;
        private decimal inputQuantity;
        private int inputLeverage;
        private PositionSide inputPositionSide;
        private decimal inputLiqPrice;
        private decimal inputMarginUSDT;
        public TestExchangeProcessor()
        {
            client = new BinanceClient();
        }

        public async Task<IProcessResult<ITradeResult>> OpenPositionAsync(string symbol, decimal costAmount, int leverage, PositionSide positionSide)
        {
            TradeProcessResult result = new TradeProcessResult();
            result.Status = ProcessStatus.Success;

            string currentSymbol = symbol.Trim().ToUpper();
            if (!currentSymbol.EndsWith("USDT")) currentSymbol += "USDT";

            var priceResult = await GraphicProcessor.GetCurrentPriceAsync(currentSymbol);
            if (priceResult.Status == ProcessStatus.Fail)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = priceResult.Message;
                return result;
            }

            decimal liqPrice = priceResult.Data / leverage;
            if (positionSide == PositionSide.Long) liqPrice = (decimal)priceResult.Data - liqPrice;
            else liqPrice = priceResult.Data + liqPrice;

            var marginUsdtResult = await GraphicProcessor.GetUSDTFromAssetAsync(currentSymbol, costAmount);
            if (marginUsdtResult.Status == ProcessStatus.Fail)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = marginUsdtResult.Message;
                return result;
            }

            TradeResult tradeData = new TradeResult();

            tradeData.Price = (decimal)priceResult.Data;
            tradeData.TimeStamp = DateTime.Now;
            tradeData.PNL = 0;
            tradeData.Symbol = currentSymbol;
            tradeData.FeeUSDT = 0;

            result.Data = tradeData;

            inputQuantity = costAmount;
            inputLeverage = leverage;
            inputPositionSide = positionSide;
            inputLiqPrice = liqPrice;
            inputMarginUSDT = marginUsdtResult.Data;

            return result;
        }

        public async Task<IProcessResult<ITradeResult>> ClosePositionAsync(ITradeResult openedPosition)
        {
            TradeProcessResult result = new TradeProcessResult();
            result.Status = ProcessStatus.Success;

            if (openedPosition == null)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = "Not opened any position in yet";
                return result;
            }

            var priceResult = await GraphicProcessor.GetCurrentPriceAsync(openedPosition.Symbol);
            if (priceResult.Status == ProcessStatus.Fail)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = priceResult.Message;
                return result;
            }

            var percentResult = TradeHelpers.PercentChange(openedPosition.Price, priceResult.Data);
            if (percentResult.Status == ProcessStatus.Fail)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = percentResult.Message;
                return result;
            }

            decimal pnl = inputMarginUSDT * (percentResult.Data * inputLeverage) / 100;
            if (inputPositionSide == PositionSide.Short) pnl *= -1;

            TradeResult tradeData = new TradeResult();

            tradeData.Price = priceResult.Data;
            tradeData.TimeStamp = DateTime.Now;
            tradeData.PNL = pnl;
            tradeData.Symbol = openedPosition.Symbol;
            tradeData.FeeUSDT = 0;

            result.Data = tradeData;

            return result;
        }

        public async Task<IProcessResult<IPositionResult>> GetPositionDataAsync(ITradeResult openedPosition)
        {
            PositionProcessResult result = new PositionProcessResult();
            result.Status = ProcessStatus.Success;

            if (openedPosition == null)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = "Not opened any position in yet";
                return result;
            }

            var priceResult = await GraphicProcessor.GetCurrentPriceAsync(openedPosition.Symbol);
            if (priceResult.Status == ProcessStatus.Fail)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = priceResult.Message;
                return result;
            }

            var percentResult = TradeHelpers.PercentChange(openedPosition.Price, priceResult.Data);
            if (percentResult.Status == ProcessStatus.Fail)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = percentResult.Message;
                return result;
            }

            decimal roe = percentResult.Data * inputLeverage;
            decimal pnl = inputMarginUSDT * roe / 100;
            if (inputPositionSide == PositionSide.Short)
            {
                pnl *= -1;
                roe *= -1;
            }

            PositionResult positionData = new PositionResult();

            positionData.Symbol = openedPosition.Symbol;
            positionData.EntryTime = openedPosition.TimeStamp;
            positionData.Side = inputPositionSide;
            positionData.PNL = pnl; // as USDT
            positionData.ROE = roe;
            positionData.MarginUSDT = inputMarginUSDT; // as USDT
            positionData.LiquidationPrice = inputLiqPrice;
            positionData.Leverage = inputLeverage;
            positionData.EntryPrice = openedPosition.Price;
            positionData.MarkPrice = priceResult.Data;
            positionData.Quantity = inputQuantity;

            result.Data = positionData;

            return result;
        }
    }
}
