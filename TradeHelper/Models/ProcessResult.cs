using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHelper.Interfaces;
using static TradeHelper.Enums.EnumLibrary;

namespace TradeHelper.Models
{
    internal class ProcessResult : IProcessResult
    {
        public ProcessStatus Status { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}
