using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TradeHelper.Enums.EnumLibrary;

namespace TradeHelper.Interfaces
{
    public interface IProcessResult<T>
    {
        ProcessStatus Status { get; set; }
        string Message { get; set; }
        T Data { get; set; }
    }

    public interface IProcessResult
    {
        ProcessStatus Status { get; set; }
        string Message { get; set; }
    }
}
