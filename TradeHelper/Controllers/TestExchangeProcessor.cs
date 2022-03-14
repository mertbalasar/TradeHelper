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
        public TestExchangeProcessor()
        {
            client = new BinanceClient();
        }

        public async Task<IProcessResult> OpenPositionAsync(string symbol, decimal costAmount, int leverage, PositionType positionType)
        {
            ProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;

            OrderSide side;
            if (positionType == PositionType.Long) side = OrderSide.Buy;
            else side = OrderSide.Sell;

            string currentSymbol = symbol.Trim().ToUpper();
            if (!currentSymbol.EndsWith("USDT")) currentSymbol += "USDT";

            IProcessResult priceResult = await GraphicProcessor.GetCurrentPriceAsync(currentSymbol);
            if (priceResult.Status == ProcessStatus.Fail)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = priceResult.Message;
                return result;
            }

            decimal liqPrice = (decimal)priceResult.Data / leverage;
            if (positionType == PositionType.Long) liqPrice = (decimal)priceResult.Data - liqPrice;
            else liqPrice = (decimal)priceResult.Data + liqPrice;

            PositionResult positionData = new PositionResult();

            positionData.Symbol = currentSymbol;
            positionData.EntryTime = DateTime.Now;
            positionData.Side = side;
            positionData.PNL = 0; // as USDT
            positionData.ROE = 0;
            positionData.MarginUSDT = costAmount * (decimal)priceResult.Data; // as USDT
            positionData.LiqPrice = liqPrice;
            positionData.Leverage = leverage;
            positionData.EntryPrice = (decimal)priceResult.Data;
            positionData.MarkPrice = (decimal)priceResult.Data;
            positionData.AddedAmount = costAmount;

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

            IProcessResult priceResult = await GraphicProcessor.GetCurrentPriceAsync(openedPosition.Symbol);
            if (priceResult.Status == ProcessStatus.Fail)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = priceResult.Message;
                return result;
            }

            IProcessResult percentResult = TradeHelpers.PercentChange(openedPosition.EntryPrice, (decimal)priceResult.Data);
            if (percentResult.Status == ProcessStatus.Fail)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = percentResult.Message;
                return result;
            }

            decimal pnl = openedPosition.MarginUSDT * ((decimal)percentResult.Data * openedPosition.Leverage) / 100;
            if (openedPosition.Side == OrderSide.Sell) pnl *= -1;

            TradeResult tradeData = new TradeResult();

            tradeData.ClosePrice = (decimal)priceResult.Data;
            tradeData.CloseTime = DateTime.Now;
            tradeData.PNL = pnl;
            tradeData.Symbol = openedPosition.Symbol;
            tradeData.FeeUSDT = 0;

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

        public async Task<IProcessResult> GetPositionDataAsync(IPositionResult openedPosition)
        {
            ProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;

            if (openedPosition == null)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = "Not opened any position in yet";
                return result;
            }

            IProcessResult priceResult = await GraphicProcessor.GetCurrentPriceAsync(openedPosition.Symbol);
            if (priceResult.Status == ProcessStatus.Fail)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = priceResult.Message;
                return result;
            }

            IProcessResult percentResult = TradeHelpers.PercentChange(openedPosition.EntryPrice, (decimal)priceResult.Data);
            if (percentResult.Status == ProcessStatus.Fail)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = percentResult.Message;
                return result;
            }

            decimal roe = (decimal)percentResult.Data * openedPosition.Leverage;
            decimal pnl = openedPosition.MarginUSDT * roe / 100;
            if (openedPosition.Side == OrderSide.Sell) pnl *= -1;

            PositionResult positionData = new PositionResult();

            positionData.Symbol = openedPosition.Symbol;
            positionData.EntryTime = openedPosition.EntryTime;
            positionData.Side = openedPosition.Side;
            positionData.PNL = pnl; // as USDT
            positionData.ROE = roe;
            positionData.MarginUSDT = openedPosition.MarginUSDT; // as USDT
            positionData.LiqPrice = openedPosition.LiqPrice;
            positionData.Leverage = openedPosition.Leverage;
            positionData.EntryPrice = openedPosition.EntryPrice;
            positionData.MarkPrice = (decimal)priceResult.Data;
            positionData.AddedAmount = openedPosition.AddedAmount;

            result.Data = positionData;

            return result;
        }
    }
}
