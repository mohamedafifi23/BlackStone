using API.Errors;
using Core.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class PaymentController : BaseApiController
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet("CardPayUrl")]
        public async Task<IActionResult> GetCardPayUrl()
        {
            //get user using claims
            var url = await _paymentService.CreateIFrameUrlForCardPayment();

            if (url == null) return BadRequest(new ApiErrorResponse(400));

            return Ok(url);
        }

        [HttpGet("WalletPayUrl")]
        public async Task<IActionResult> GetWalletPayUrl()
        {
            //get user using claims
            var url = await _paymentService.CreateWalletRedirectUrl();

            if (url == null) return BadRequest(new ApiErrorResponse(400));

            return Ok(url);
        }
    }
}
