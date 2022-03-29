using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHelper.Interfaces
{
    public interface INotificationProcessor
    {
        IProcessResult SendMail(string subject, string content);
    }
}
