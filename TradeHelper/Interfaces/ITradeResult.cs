using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHelper.Interfaces
{
    public interface ITradeResult
    {
        string Symbol { get; set; }
        decimal PNL { get; set; }
        DateTime CloseTime { get; set; }
        decimal ClosePrice { get; set; }
        decimal FeeUSDT { get; set; }
    }
}
