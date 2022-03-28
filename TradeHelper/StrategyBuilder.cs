using Binance.Net.Enums;
using Binance.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private static List<IBindStrategy> Strategies { get; set; } = new List<IBindStrategy>();
        private static bool runAlwaysFlag;

        public static IProcessResult Append(Type strategyClass)
        {
            Strategy strategy = (Strategy)Activator.CreateInstance(strategyClass);

            IProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;

            IBindStrategy bound = Strategies.ToList().Where((element) => element.Strategy.GetType() == strategy.GetType()).FirstOrDefault();
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

                Strategies.Add(new BindStrategy() { Strategy = strategy, Trigger = trigger });
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
                foreach (IBindStrategy bounds in Strategies)
                {
                    await StartedStrategy(bounds);

                    KlineInterval intervalParam = KlineInterval.OneMinute;
                    if (bounds.Strategy.RunTriggeredInterval != null)
                    {
                        intervalParam = (KlineInterval)bounds.Strategy.RunTriggeredInterval;
                    }
                    bounds.Trigger.Start(intervalParam);
                }
                runAlwaysFlag = true;
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
                foreach (IBindStrategy bounds in Strategies)
                {
                    bounds.Trigger.Stop();
                }
                runAlwaysFlag = false;
                ReportProcessor.ResetMembers();
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

            IBindStrategy bound = Strategies.ToList().Where((element) => element.Strategy.GetType() == strategy.GetType()).FirstOrDefault();
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

        private async static Task<bool> StartedStrategy(IBindStrategy bounds)
        {
            bounds.Strategy.Binance = new BinanceProcessor() { GMTForGraph = bounds.Strategy.GMTForGraph };
            bounds.Strategy.Test = new TestExchangeProcessor();
            await bounds.Strategy.Initialize();

            Task.Run(async () =>
            {
                while (runAlwaysFlag)
                {
                    await bounds.Strategy.RunAlways();
                    await Task.Delay(bounds.Strategy.RunAlwaysDelay);
                }
            });

            return true;
        }

        private static async void Trigger_Triggered(object sender, Strategy strategy)
        {
            await Task.Delay(800);

            KlineInterval intervalParam = KlineInterval.OneMinute;
            if (strategy.RunTriggeredInterval != null)
            {
                intervalParam = (KlineInterval)strategy.RunTriggeredInterval;
            }

            List<string> symbolParam = new List<string>();
            string currentSymbol;
            if (strategy.Symbols != null)
            {
                foreach (string symbol in strategy.Symbols)
                {
                    currentSymbol = symbol.Trim().ToUpper();
                    if (!currentSymbol.EndsWith("USDT")) currentSymbol += "USDT";
                    if (symbolParam.Where((element) => element.Equals(currentSymbol)).FirstOrDefault() == null) symbolParam.Add(currentSymbol);
                }
            }

            IProcessResult klineResult = await GraphicProcessor.GetKlinesAsync(symbolParam.ToArray(), intervalParam, gmt: strategy.GMTForGraph);

            strategy.RunTriggered(klineResult);
        }
    }
}
