using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHelper.AbstractClasses;
using TradeHelper.Interfaces;
using static TradeHelper.Enums.EnumLibrary;

namespace TradeHelper.Models
{
    internal class ProcessResult : IProcessResult
    {
        public ProcessStatus Status { get; set; }
        public string Message { get; set; }
    }

    internal class DecimalProcessResult : IProcessResult<decimal>
    {
        public ProcessStatus Status { get; set; }
        public string Message { get; set; }
        public decimal Data { get; set; }
    }

    internal class PositionProcessResult : IProcessResult<IPositionResult>
    {
        public ProcessStatus Status { get; set; }
        public string Message { get; set; }
        public IPositionResult Data { get; set; }
    }

    internal class TradeProcessResult : IProcessResult<ITradeResult>
    {
        public ProcessStatus Status { get; set; }
        public string Message { get; set; }
        public ITradeResult Data { get; set; }
    }

    internal class OrderProcessResult : IProcessResult<IOrderResult>
    {
        public ProcessStatus Status { get; set; }
        public string Message { get; set; }
        public IOrderResult Data { get; set; }
    }

    internal class OrderLocationProcessResult : IProcessResult<OrderLocation>
    {
        public ProcessStatus Status { get; set; }
        public string Message { get; set; }
        public OrderLocation Data { get; set; }
    }

    internal class QuoteProcessResult : IProcessResult<List<Quote>>
    {
        public ProcessStatus Status { get; set; }
        public string Message { get; set; }
        public List<Quote> Data { get; set; }
    }

    internal class KlineProcessResult : IProcessResult<List<IKlineResult>>
    {
        public ProcessStatus Status { get; set; }
        public string Message { get; set; }
        public List<IKlineResult> Data { get; set; }
    }

    internal class StringListProcessResult : IProcessResult<List<string>>
    {
        public ProcessStatus Status { get; set; }
        public string Message { get; set; }
        public List<string> Data { get; set; }
    }

    internal class StrategyProcessResult : IProcessResult<Strategy>
    {
        public ProcessStatus Status { get; set; }
        public string Message { get; set; }
        public Strategy Data { get; set; }
    }
}
