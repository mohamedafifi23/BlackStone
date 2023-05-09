using API.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Reflection;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IStringLocalizer<SharedResource> _sharedResourceLocalizer;       

        public WeatherForecastController(ILogger<WeatherForecastController> logger,
            IStringLocalizer<SharedResource> sharedResourceLocalizer)
        {
            _logger = logger;
            _sharedResourceLocalizer = sharedResourceLocalizer;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            _logger.LogError("kkkkkkkkh");
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
            //var res = _localizer2.GetString("hello").Value ?? "";

            //return _sharedResourceLocalizer["hello"];
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
    }
}