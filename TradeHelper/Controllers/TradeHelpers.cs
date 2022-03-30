﻿using Binance.Net.Clients;
using Binance.Net.Interfaces;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHelper.Interfaces;
using TradeHelper.Models;
using static TradeHelper.Enums.EnumLibrary;

namespace TradeHelper.Controllers
{
    public static class TradeHelpers
    {
        private static BinanceClient client = new BinanceClient();

        public static async Task<IProcessResult> GetLotSizeFilterAsync(string symbol)
        {
            ProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;

            var decimalResult = await client.UsdFuturesApi.ExchangeData.GetExchangeInfoAsync();
            if (!decimalResult.Success)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = decimalResult.Error.Message;
                return result;
            }

            result.Data = decimalResult.Data.Symbols.ToList().Where((element) => element.BaseAsset.Equals(symbol.Replace("USDT", ""))).First().LotSizeFilter.StepSize;

            return result;
        }

        public static IProcessResult PercentChange(decimal firstPrice, decimal lastPrice)
        {
            IProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;

            if (firstPrice == 0)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = "The 'firstPrice' parameter can not be zero";
                return result;
            }

            result.Data = ((lastPrice - firstPrice) / firstPrice) * 100;

            return result;
        }

        public static IProcessResult PriceChange(decimal price, decimal percentChange)
        {
            IProcessResult result = new ProcessResult();

            result.Status = ProcessStatus.Success;
            result.Data = ((percentChange / 100) * price) + price;

            return result;
        }

        public static List<Quote> GetQuotes(List<IBinanceKline> allKlines)
        {
            List<Quote> quotes = new List<Quote>();

            foreach (IBinanceKline kline in allKlines)
            {
                quotes.Add(new Quote()
                {
                    Open = kline.OpenPrice,
                    High = kline.HighPrice,
                    Close = kline.ClosePrice,
                    Low = kline.LowPrice,
                    Date = kline.OpenTime
                });
            }

            return quotes;
        }

        public static async Task<IProcessResult> GetConnectionStatus()
        {
            IProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;

            var priceResult = await client.UsdFuturesApi.ExchangeData.GetPriceAsync("BTCUSDT");
            if (!priceResult.Success && priceResult.Error.Message.ToLower().Contains("an error occurred while sending the request"))
            {
                result.Status = ProcessStatus.Fail;
                result.Message = "No internet connection";
            }

            return result;
        }

        public static IProcessResult GetPrecisionDecimal(decimal number)
        {
            IProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;
            result.Data = BitConverter.GetBytes(decimal.GetBits(number)[3])[2];

            return result;
        }
    }
}
