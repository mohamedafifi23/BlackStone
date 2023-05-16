using Core.ServiceHelpers.EmailSenderService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.IServices
{
    public interface IEmailSenderService
    {
        void SendEmail(Message message);
        Task SendEmailAsync(Message message);
    }
}
