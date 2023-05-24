using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Core.ServiceHelpers.PaymentService
{
    public class WalletPayRequestResponse
    {
        public bool Success { get; set; }

        public bool Pending { get; set; }

        [JsonPropertyName("redirect_url")]
        public string RedirectUrl { get; set; }        
    }
}
