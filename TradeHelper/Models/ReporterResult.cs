using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHelper.Interfaces;

namespace TradeHelper.Models
{
    internal class ReporterResult : IReporterResult
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan TotalElapsedTime { get; set; }
        public int TotalPositionCount { get; set; }
        public int ProfitedPositionCount { get; set; }
        public int LossedPositionCount { get; set; }
        public decimal AccuracyRatio { get; set; }
        public decimal RealizedPNL { get; set; }
        public decimal TotalFee { get; set; }
        public List<IReporterDetailsResult> Details { get; set; }
    }
}
