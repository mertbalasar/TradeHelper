using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHelper.Interfaces
{
    internal interface IReporterResult
    {
        DateTime StartTime { get; set; }
        DateTime EndTime { get; set; }
        TimeSpan TotalElapsedTime { get; set; }
        int TotalPositionCount { get; set; }
        int ProfitedPositionCount { get; set; }
        int LossedPositionCount { get; set; }
        decimal AccuracyRatio { get; set; }
        decimal RealizedPNL { get; set; }
        decimal TotalFee { get; set; }
        List<IReporterDetailsResult> Details { get; set; }
    }
}
