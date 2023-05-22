using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ServiceHelpers.PaymentService
{
    public class PaymobConfiguration
    {
        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }
        public string IntegrationId { get; set; }
        public string LockOrderWhenPaid { get; set; }
        public string IFrameUrl { get; set; }
        public string IFrameId { get; set; }
        
    }
}
