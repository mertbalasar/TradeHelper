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
    internal class ReportProcessor : IReportProcessor
    {
        private string path = null;
        private IReporterResult report = null;

        public IProcessResult ResetMembers()
        {
            path = null;
            report = null;

            ProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;
            return result;
        }

        public IProcessResult AddReportForOpenPosition(ITradeResult openedPosition)
        {
            ProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;

            if (report == null)
            {
                report = new ReporterResult();
                report.StartTime = openedPosition.TimeStamp;
                report.Details = new List<IReporterDetailsResult>();
            }

            report.Details.Add(new ReporterDetailsResult() { OpenPosition = openedPosition });
            report.EndTime = openedPosition.TimeStamp;
            report.TotalElapsedTime = report.EndTime - report.StartTime;
            report.TotalFee += openedPosition.FeeUSDT;

            IProcessResult saveResult = SaveReport(report);
            if (saveResult.Status == ProcessStatus.Fail)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = saveResult.Message;
                return result;
            }

            return result;
        }

        public IProcessResult AddReportForClosePosition(ITradeResult closedPosition)
        {
            ProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;

            if (report == null)
            {
                report = new ReporterResult();
                report.StartTime = closedPosition.TimeStamp;
                report.Details = new List<IReporterDetailsResult>();
            }

            report.Details.Add(new ReporterDetailsResult() { ClosePosition = closedPosition });
            report.EndTime = closedPosition.TimeStamp;
            report.TotalElapsedTime = report.EndTime - report.StartTime;
            report.TotalFee += closedPosition.FeeUSDT;
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
            report.TotalPNLWithoutReducedFee += closedPosition.PNL;
            report.TotalPNLWithReducedFee = report.TotalPNLWithoutReducedFee - report.TotalFee;
            
            IProcessResult saveResult = SaveReport(report);
            if (saveResult.Status == ProcessStatus.Fail)
            {
                result.Status = ProcessStatus.Fail;
                result.Message = saveResult.Message;
                return result;
            }

            return result;
        }

        private IProcessResult SaveReport(IReporterResult data)
        {
            ProcessResult result = new ProcessResult();
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
