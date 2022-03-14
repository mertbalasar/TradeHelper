using Binance.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHelper.AbstractClasses;
using static TradeHelper.Enums.EnumLibrary;

namespace TradeHelper.Controllers
{
    internal class TriggerProcessor
    {
        internal delegate void TriggerHandler(object sender, Strategy strategy);
        internal event TriggerHandler Triggered;
        internal Strategy Strategy { get; set; }
        private bool status = false;
        private bool allow;
        private bool? now = null, before = null;

        private bool Control(KlineInterval interval)
        {
            allow = false;

            switch (interval)
            {
                case KlineInterval.OneMinute:
                    if (DateTime.Now.Minute % 1 == 0 && DateTime.Now.Second == 0)
                    {
                        allow = true;
                    }
                    break;
                case KlineInterval.ThreeMinutes:
                    if (DateTime.Now.Minute % 3 == 0 && DateTime.Now.Second == 0)
                    {
                        allow = true;
                    }
                    break;
                case KlineInterval.FiveMinutes:
                    if (DateTime.Now.Minute % 5 == 0 && DateTime.Now.Second == 0)
                    {
                        allow = true;
                    }
                    break;
                case KlineInterval.FifteenMinutes:
                    if (DateTime.Now.Minute % 15 == 0 && DateTime.Now.Second == 0)
                    {
                        allow = true;
                    }
                    break;
                case KlineInterval.ThirtyMinutes:
                    if (DateTime.Now.Minute % 30 == 0 && DateTime.Now.Second == 0)
                    {
                        allow = true;
                    }
                    break;
                case KlineInterval.OneHour:
                    if (DateTime.Now.Hour % 1 == 0 && DateTime.Now.Minute == 0 && DateTime.Now.Second == 0)
                    {
                        allow = true;
                    }
                    break;
                case KlineInterval.TwoHour:
                    if (DateTime.Now.Hour % 2 == 0 && DateTime.Now.Minute == 0 && DateTime.Now.Second == 0)
                    {
                        allow = true;
                    }
                    break;
                case KlineInterval.FourHour:
                    if (DateTime.Now.Hour % 4 == 0 && DateTime.Now.Minute == 0 && DateTime.Now.Second == 0)
                    {
                        allow = true;
                    }
                    break;
                case KlineInterval.SixHour:
                    if (DateTime.Now.Hour % 6 == 0 && DateTime.Now.Minute == 0 && DateTime.Now.Second == 0)
                    {
                        allow = true;
                    }
                    break;
                case KlineInterval.EightHour:
                    if (DateTime.Now.Hour % 8 == 0 && DateTime.Now.Minute == 0 && DateTime.Now.Second == 0)
                    {
                        allow = true;
                    }
                    break;
                case KlineInterval.TwelveHour:
                    if (DateTime.Now.Hour % 20 == 0 && DateTime.Now.Minute == 0 && DateTime.Now.Second == 0)
                    {
                        allow = true;
                    }
                    break;
                case KlineInterval.OneDay:
                    if (DateTime.Now.Day % 1 == 0 && DateTime.Now.Hour == 0 && DateTime.Now.Minute == 0 && DateTime.Now.Second == 0)
                    {
                        allow = true;
                    }
                    break;
                case KlineInterval.ThreeDay:
                    if (DateTime.Now.Day % 3 == 0 && DateTime.Now.Hour == 0 && DateTime.Now.Minute == 0 && DateTime.Now.Second == 0)
                    {
                        allow = true;
                    }
                    break;
            }

            return allow;
        }

        internal void Start(KlineInterval interval)
        {
            status = true;

            Task.Run(() =>
            {
                while (status)
                {
                    now = Control(interval);
                    if (before == null) before = !now;

                    if ((bool)before != (bool)now && (bool)now && Triggered != null) Triggered(new object(), Strategy);

                    before = now;
                }
            });
        }

        internal void Stop()
        {
            status = false;
        }
    }
}
