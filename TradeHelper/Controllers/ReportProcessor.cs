using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHelper.Interfaces;
using TradeHelper.Models;
using static TradeHelper.Enums.EnumLibrary;

namespace TradeHelper.Controllers
{
    internal static class ReportProcessor
    {
        private static string path = null;
        private static IReporterResult report = null;

        public static IProcessResult ResetMembers()
        {
            path = null;
            report = null;

            IProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;
            return result;
        }

        public static IProcessResult AddReport(IPositionResult openedPosition)
        {
            IProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;

            if (report == null)
            {
                report = new ReporterResult();
                report.StartTime = openedPosition.EntryTime;
                report.Details = new List<IReporterDetailsResult>();
            }

            report.Details.Add(new ReporterDetailsResult() { OpenPosition = openedPosition });
            report.EndTime = openedPosition.EntryTime;
            report.TotalElapsedTime = report.EndTime - report.StartTime;

            IProcessResult saveResult = SaveReport(report);
            if (saveResult.Status == ProcessStatus.Fail)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = saveResult.Message;
                return result;
            }

            return result;
        }

        public static IProcessResult AddReport(ITradeResult closedPosition)
        {
            IProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;

            if (report == null)
            {
                report = new ReporterResult();
                report.StartTime = closedPosition.CloseTime;
                report.Details = new List<IReporterDetailsResult>();
            }

            report.Details.Add(new ReporterDetailsResult() { ClosePosition = closedPosition });
            report.EndTime = closedPosition.CloseTime;
            report.TotalElapsedTime = report.EndTime - report.StartTime;
            report.TotalPositionCount += 1;

            if (closedPosition.PNL - closedPosition.FeeUSDT > 0)
            {
                report.ProfitedPositionCount += 1;
            }
            else if (closedPosition.PNL - closedPosition.FeeUSDT <= 0)
            {
                report.LossedPositionCount += 1;
            }

            if (report.TotalPositionCount > 0)
            {
                report.AccuracyRatio = (100 * report.ProfitedPositionCount) / report.TotalPositionCount;
            }           
            report.RealizedPNL += closedPosition.PNL - closedPosition.FeeUSDT;
            report.TotalFee = closedPosition.FeeUSDT;

            IProcessResult saveResult = SaveReport(report);
            if (saveResult.Status == ProcessStatus.Fail)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = saveResult.Message;
                return result;
            }

            return result;
        }

        private static IProcessResult SaveReport(IReporterResult data)
        {
            IProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;

            if (string.IsNullOrEmpty(path))
            {
                DateTime now = DateTime.Now;
                path = @"./Reports/Result_" +
                        now.ToString("dd") +
                        now.ToString("MM") +
                        now.ToString("yy") + "_" +
                        now.ToString("HH") +
                        now.ToString("mm") +
                        now.ToString("ss") +
                        ".json";
            }
            
            try
            {
                if (!Directory.Exists("Reports")) Directory.CreateDirectory("Reports");

                string json = JsonConvert.SerializeObject(data);               
                File.WriteAllText(path, json);
            }
            catch (Exception e)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = e.Message;
            }

            return result;
        }
    }
}
