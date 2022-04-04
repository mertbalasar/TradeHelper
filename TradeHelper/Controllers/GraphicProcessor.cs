using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Spot;
using Skender.Stock.Indicators;
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
    public static class GraphicProcessor
    {
        private static BinanceClient client = new BinanceClient();

        public static async Task<IProcessResult<decimal>> GetUSDTFromAssetAsync(string symbol, decimal amountAsset)
        {
            DecimalProcessResult result = new DecimalProcessResult();
            result.Status = ProcessStatus.Success;

            string currentSymbol = symbol.Trim().ToUpper();
            if (!currentSymbol.EndsWith("USDT")) currentSymbol += "USDT";

            var priceResult = await client.UsdFuturesApi.ExchangeData.GetPriceAsync(currentSymbol);
            if (!priceResult.Success)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = priceResult.Error.Message;
                return result;
            }

            result.Data = priceResult.Data.Price * amountAsset;

            return result;
        }

        public static async Task<IProcessResult<decimal>> GetAssetFromUSDTAsync(string symbol, decimal amountUSDT)
        {
            DecimalProcessResult result = new DecimalProcessResult();
            result.Status = ProcessStatus.Success;

            string currentSymbol = symbol.Trim().ToUpper();
            if (!currentSymbol.EndsWith("USDT")) currentSymbol += "USDT";

            var priceResult = await client.UsdFuturesApi.ExchangeData.GetPriceAsync(currentSymbol);
            if (!priceResult.Success)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = priceResult.Error.Message;
                return result;
            }

            result.Data = amountUSDT / priceResult.Data.Price;

            return result;
        }

        public static async Task<IProcessResult<List<IKlineResult>>> GetKlinesAsync(string[] symbols, KlineInterval interval, int gmt = 0)
        {
            KlineProcessResult result = new KlineProcessResult();
            result.Status = ProcessStatus.Success;

            List<IKlineResult> klines = new List<IKlineResult>();
            List<Quote> quotes;
            string currentSymbol;
            foreach (string element in symbols)
            {
                currentSymbol = element.Trim().ToUpper();
                if (!currentSymbol.EndsWith("USDT")) currentSymbol += "USDT";

                var klineResult = await client.UsdFuturesApi.ExchangeData.GetKlinesAsync(currentSymbol, interval);
                if (!klineResult.Success)
                {
                    result.Status = ProcessStatus.Fail;
                    result.Message = klineResult.Error.Message;
                    return result;
                }

                foreach (IBinanceKline kline in klineResult.Data.ToList())
                {
                    kline.CloseTime = kline.CloseTime.AddHours(gmt);
                    kline.OpenTime = kline.OpenTime.AddHours(gmt);
                }

                quotes = TradeHelpers.GetQuotes(klineResult.Data.ToList()).Data;

                foreach (Quote kline in quotes)
                {
                    kline.Date = kline.Date.AddHours(gmt);
                }

                klines.Add(new KlineResult() { Symbol = currentSymbol, Klines = klineResult.Data.ToList(), Indicators = quotes });
            }

            result.Data = klines;
            
            return result;
        }

        public static async Task<IProcessResult<List<IKlineResult>>> GetKlinesAsync(KlineInterval interval, int gmt = 0)
        {
            KlineProcessResult result = new KlineProcessResult();
            result.Status = ProcessStatus.Success;

            var pricesResult = await client.UsdFuturesApi.ExchangeData.GetPricesAsync();
            if (!pricesResult.Success)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = pricesResult.Error.Message;
                return result;
            }

            List<IKlineResult> klines = new List<IKlineResult>();
            List<Quote> quotes;
            foreach (BinancePrice element in pricesResult.Data.ToList())
            {
                var klineResult = await client.UsdFuturesApi.ExchangeData.GetKlinesAsync(element.Symbol, interval);
                if (!klineResult.Success)
                {
                    result.Status = ProcessStatus.Fail;
                    result.Message = klineResult.Error.Message;
                    return result;
                }

                foreach (IBinanceKline kline in klineResult.Data.ToList())
                {
                    kline.CloseTime = kline.CloseTime.AddHours(gmt);
                    kline.OpenTime = kline.OpenTime.AddHours(gmt);
                }

                quotes = TradeHelpers.GetQuotes(klineResult.Data.ToList()).Data;

                foreach (Quote kline in quotes)
                {
                    kline.Date = kline.Date.AddHours(gmt);
                }

                klines.Add(new KlineResult() { Symbol = element.Symbol, Klines = klineResult.Data.ToList(), Indicators = quotes });
            }

            result.Data = klines;

            return result;
        }

        public static async Task<IProcessResult<decimal>> GetCurrentPriceAsync(string symbol)
        {
            DecimalProcessResult result = new DecimalProcessResult();
            result.Status = ProcessStatus.Success;

            string currentSymbol = symbol.Trim().ToUpper();
            if (!currentSymbol.EndsWith("USDT")) currentSymbol += "USDT";

            var priceResult = await client.UsdFuturesApi.ExchangeData.GetPriceAsync(currentSymbol);
            if (!priceResult.Success)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = priceResult.Error.Message;
                return result;
            }

            result.Data = priceResult.Data.Price;

            return result;
        }

        public static async Task<IProcessResult<List<string>>> GetAllSymbolsAsync()
        {
            StringListProcessResult result = new StringListProcessResult();
            result.Status = ProcessStatus.Success;

            var priceResult = await client.UsdFuturesApi.ExchangeData.GetPricesAsync();
            if (!priceResult.Success)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = priceResult.Error.Message;
                return result;
            }

            List<string> symbols = new List<string>();
            foreach (BinancePrice element in priceResult.Data.ToList())
            {
                symbols.Add(element.Symbol);
            }

            result.Data = symbols;

            return result;
        }
    }
}
