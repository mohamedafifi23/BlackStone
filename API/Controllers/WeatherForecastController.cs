using API.Dtos;
using Core.IServices;
using Core.ServiceHelpers.EmailSenderService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Data;
using System.Reflection;

namespace API.Controllers
{

    public class WeatherForecastController : BaseApiController
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IStringLocalizer<SharedResource> _sharedResourceLocalizer;
        private readonly IEmailSenderService _emailService;
        private readonly IPaymentService _paymentService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,
            IStringLocalizer<SharedResource> sharedResourceLocalizer, IEmailSenderService emailService
            , IPaymentService paymentService)
        {
            _logger = logger;
            _sharedResourceLocalizer = sharedResourceLocalizer;
            _emailService = emailService;
            _paymentService = paymentService;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            _logger.LogError("kkkkkkkkh");
            _emailService.SendEmail(new Message(new List<string> { "mohammedafifi153@gmail.com"}, "test email service", "test is successful or not"));
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("TestLocalization")]
        public string GetLocalization()
        {
                var res = _sharedResourceLocalizer["hello"];
            try
            {
                //var res = _localizer2.GetString("hello").Value ?? "";
                //return _sharedResourceLocalizer["hello"];
                throw new NullReferenceException();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return _sharedResourceLocalizer["hello"];
        }

        [HttpPost("TestLocalizationDA")]
        public string Add(TestDto dto)
        {            
            _logger.LogInformation(DateTime.Now.ToLongTimeString());

            var res = _sharedResourceLocalizer["hello"];
            //var res = _localizer2.GetString("hello").Value ?? "";

            return _sharedResourceLocalizer["hello"];
        }

        [HttpGet("testerror")]
        public async Task<IActionResult> TestError()
        {
            string s = null;
            s.ToString();
            return Content("test");
        }

        [HttpGet("GetPaymobToken")]
        public async Task<IActionResult> GetPaymobToken()
        {
            var token = await _paymentService.GetPaymentToken();
            return Ok(token);
        }

        [HttpGet("GetIframeCardUrl")]
        public async Task<IActionResult> GetPaymobToken2()
        {
            var token = await _paymentService.CreateIFrameUrlForCardPayment();
            return Ok(token);
        }

        [HttpGet("GetWalletUrl")]
        public async Task<IActionResult> GetWalletUrl()
        {
            var token = await _paymentService.CreateWalletRedirectUrl();
            return Ok(token);
        }

    }
}