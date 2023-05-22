﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.IServices
{
    public interface IPaymentService
    {
        Task<string> GetPaymentToken();
        Task<string> CreateIFrameUrlForCardPayment();
        //string GenerateIFrameUrlForBankPayment();
    }
}
