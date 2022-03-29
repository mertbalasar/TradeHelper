using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using TradeHelper.Interfaces;
using TradeHelper.Models;
using static TradeHelper.Enums.EnumLibrary;
using System.Net;

namespace TradeHelper.Controllers
{
    internal class NotificationProcessor : INotificationProcessor
    {
        internal MailMessage mailMessage;
        internal SmtpClient smtp;
        internal string[] MailList { get; set; }

        public IProcessResult SendMail(string subject, string content)
        {
            IProcessResult result = new ProcessResult();
            result.Status = ProcessStatus.Success;

            try
            {
                mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("tradehelperofficial@gmail.com");

                foreach (string address in MailList)
                {
                    mailMessage.To.Add(address);
                }

                mailMessage.Subject = subject;
                mailMessage.Body = content;

                smtp = new SmtpClient();

                smtp.Credentials = new NetworkCredential("tradehelperofficial@gmail.com", "czninbabaannesi");
                smtp.Port = 587;
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                smtp.Send(mailMessage);
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
