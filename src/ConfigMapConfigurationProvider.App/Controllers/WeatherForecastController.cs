using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ConfigMapConfigurationProvider.App.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly IOptionsMonitor<WeatherForecastSettings> _options;

        public WeatherForecastController(IOptionsMonitor<WeatherForecastSettings> options)
        {
            _options = options;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, _options.CurrentValue.SampleSize.GetValueOrDefault()).Select(index =>
            {
                var forecast = new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                };

                if (_options.CurrentValue.IncludeFahrenheit is true)
                {
                    forecast.TemperatureF = 32 + (int)(forecast.TemperatureC / 0.5556);
                }

                return forecast;
            })
            .ToArray();
        }
    }
}