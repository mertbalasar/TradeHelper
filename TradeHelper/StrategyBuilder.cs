using Binance.Net.Enums;
using Binance.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TradeHelper.AbstractClasses;
using TradeHelper.Controllers;
using TradeHelper.Interfaces;
using TradeHelper.Models;
using static TradeHelper.Enums.EnumLibrary;

namespace TradeHelper
{
    public static class StrategyBuilder
    {
        private static AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        private static List<TriggerProcessor> Strategies { get; set; } = new List<TriggerProcessor>();

        public static IProcessResult Append(Type strategyClass)
        {
            Strategy strategy = (Strategy)Activator.CreateInstance(strategyClass);

            IProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;

            TriggerProcessor bound = Strategies.ToList().Where((element) => element.Strategy.GetType() == strategy.GetType()).FirstOrDefault();
            if (bound != null)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = "Found to given strategy in the container, so that can not appended";
                return result;
            }

            try
            {
                TriggerProcessor trigger = new TriggerProcessor();
                trigger.Strategy = strategy;
                trigger.Triggered += Trigger_Triggered;

                Strategies.Add(trigger);
            }
            catch (Exception e)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = e.Message;
            }

            return result;
        }

        public async static Task<IProcessResult> StartStrategies()
        {
            IProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;

            if (Strategies.Count == 0)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = "There is no any strategy in the container";
                return result;
            }

            try
            {
                foreach (TriggerProcessor bounds in Strategies)
                {
                    await StartedStrategy(bounds);

                    KlineInterval intervalParam = KlineInterval.OneMinute;
                    if (bounds.Strategy.Settings.RunTriggeredInterval != null)
                    {
                        intervalParam = (KlineInterval)bounds.Strategy.Settings.RunTriggeredInterval;
                    }
                    bounds.Start(intervalParam);
                }
            }
            catch (Exception e)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = e.Message;
            }

            return result;
        }

        public static IProcessResult StopStrategies()
        {
            IProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;

            if (Strategies.Count == 0)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = "There is no any strategy in the container";
                return result;
            }

            try
            {
                foreach (TriggerProcessor bounds in Strategies)
                {
                    bounds.Stop();
                    bounds.Strategy.Tools.Report.ResetMembers();
                }
            }
            catch (Exception e)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = e.Message;
            }

            return result;
        }

        public static IProcessResult Remove(Type strategyClass)
        {
            Strategy strategy = (Strategy)Activator.CreateInstance(strategyClass);

            IProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;

            TriggerProcessor bound = Strategies.ToList().Where((element) => element.Strategy.GetType() == strategy.GetType()).FirstOrDefault();
            if (bound == null)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = "Not found to given strategy in the container";
                return result;
            }

            try
            {
                Strategies.Remove(bound);
            }
            catch (Exception e)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = e.Message;
            }

            return result;
        }

        private async static Task<bool> StartedStrategy(TriggerProcessor bounds)
        {
            bounds.Strategy.Tools = new StrategyTools();
            bounds.Strategy.Tools.Binance = new BinanceProcessor() { GMTForGraph = bounds.Strategy.Settings.GMTForGraph };
            bounds.Strategy.Tools.Test = new TestExchangeProcessor();
            bounds.Strategy.Tools.Notification = new NotificationProcessor() { MailList = bounds.Strategy.Settings.MailList };
            bounds.Strategy.Tools.Report = new ReportProcessor();
            await bounds.Strategy.Initialize();

            return true;
        }

        private static async void Trigger_Triggered(object sender, Strategy strategy)
        {
            KlineInterval intervalParam = KlineInterval.OneMinute;
            if (strategy.Settings.RunTriggeredInterval != null)
            {
                intervalParam = (KlineInterval)strategy.Settings.RunTriggeredInterval;
            }

            List<string> symbolParam = new List<string>();
            string currentSymbol;
            if (strategy.Settings.Symbols != null)
            {
                foreach (string symbol in strategy.Settings.Symbols)
                {
                    currentSymbol = symbol.Trim().ToUpper();
                    if (!currentSymbol.EndsWith("USDT")) currentSymbol += "USDT";
                    if (symbolParam.Where((element) => element.Equals(currentSymbol)).FirstOrDefault() == null) symbolParam.Add(currentSymbol);
                }
            }

            var klineResult = await GraphicProcessor.GetKlinesAsync(symbolParam.ToArray(), intervalParam, gmt: strategy.Settings.GMTForGraph);

            await Task.Run(async () =>
            {
                while (true)
                {
                    if (klineResult.Data.Count > 0)
                    {
                        DateTime nowDate = DateTime.Now;
                        if (
                            klineResult.Data[0].Klines.Last().OpenTime.Year == nowDate.Year &&
                            klineResult.Data[0].Klines.Last().OpenTime.Month == nowDate.Month &&
                            klineResult.Data[0].Klines.Last().OpenTime.Day == nowDate.Day &&
                            klineResult.Data[0].Klines.Last().OpenTime.Hour == nowDate.Hour &&
                            klineResult.Data[0].Klines.Last().OpenTime.Minute == nowDate.Minute
                            )
                        {
                            break;
                        }
                        else
                        {
                            klineResult = await GraphicProcessor.GetKlinesAsync(symbolParam.ToArray(), intervalParam, gmt: strategy.Settings.GMTForGraph);
                        }

                        autoResetEvent.WaitOne(2, true);
                    }
                    else
                    {
                        break;
                    }
                }
            });
            

            await strategy.RunTriggered(klineResult);
        }
    }
}
