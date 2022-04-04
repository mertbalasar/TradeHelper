using Binance.Net.Clients;
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

        public static async Task<IProcessResult<decimal>> GetLotSizeAmountAsync(string symbol)
        {
            DecimalProcessResult result = new DecimalProcessResult();
            result.Status = ProcessStatus.Success;

            var decimalResult = await client.UsdFuturesApi.ExchangeData.GetExchangeInfoAsync();
            if (!decimalResult.Success)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = decimalResult.Error.Message;
                return result;
            }

            result.Data = decimalResult.Data.Symbols.ToList().Where((element) => element.BaseAsset.Equals(symbol.Replace("USDT", ""))).First().LotSizeFilter.MinQuantity;

            return result;
        }

        public static async Task<IProcessResult<decimal>> FilterAmountByPrecisionAsync(string symbol, decimal amount)
        {
            DecimalProcessResult result = new DecimalProcessResult();
            result.Status = ProcessStatus.Success;

            var decimalResult = await client.UsdFuturesApi.ExchangeData.GetExchangeInfoAsync();
            if (!decimalResult.Success)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = decimalResult.Error.Message;
                return result;
            }

            int precision = decimalResult.Data.Symbols.ToList().Where((element) => element.BaseAsset.Equals(symbol.Replace("USDT", ""))).First().QuantityPrecision;
            result.Data = Math.Round(amount, precision);

            return result;
        }

        public static async Task<IProcessResult<decimal>> FilterPriceByPrecisionAsync(string symbol, decimal price)
        {
            DecimalProcessResult result = new DecimalProcessResult();
            result.Status = ProcessStatus.Success;

            var decimalResult = await client.UsdFuturesApi.ExchangeData.GetExchangeInfoAsync();
            if (!decimalResult.Success)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = decimalResult.Error.Message;
                return result;
            }

            int precision = decimalResult.Data.Symbols.ToList().Where((element) => element.BaseAsset.Equals(symbol.Replace("USDT", ""))).First().PricePrecision;
            result.Data = Math.Round(price, precision);

            return result;
        }

        public static IProcessResult<decimal> PercentChange(decimal firstPrice, decimal lastPrice)
        {
            DecimalProcessResult result = new DecimalProcessResult();
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

        public static IProcessResult<decimal> PriceChange(decimal price, decimal percentChange)
        {
            DecimalProcessResult result = new DecimalProcessResult();

            result.Status = ProcessStatus.Success;
            result.Data = ((percentChange / 100) * price) + price;

            return result;
        }

        public static IProcessResult<List<Quote>> GetQuotes(List<IBinanceKline> allKlines)
        {
            QuoteProcessResult result = new QuoteProcessResult();
            result.Status = ProcessStatus.Success;

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

            result.Data = quotes;

            return result;
        }

        public static async Task<IProcessResult> GetConnectionStatusAsync()
        {
            ProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;

            var priceResult = await client.UsdFuturesApi.ExchangeData.GetPriceAsync("BTCUSDT");
            if (!priceResult.Success && priceResult.Error.Message.ToLower().Contains("an error occurred while sending the request"))
            {
                result.Status = ProcessStatus.Fail;
                result.Message = "No internet connection";
            }

            return result;
        }
    }
}
