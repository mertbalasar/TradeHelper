using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TradeHelper.Enums.EnumLibrary;

namespace TradeHelper.Interfaces
{
    public interface IProcessResult
    {
        ProcessStatus Status { get; set; }
        string Message { get; set; }
        object Data { get; set; }
    }
}
