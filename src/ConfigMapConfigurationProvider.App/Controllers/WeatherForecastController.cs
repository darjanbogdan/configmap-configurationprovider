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

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IOptionsMonitor<TestSettings> _options;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IOptionsMonitor<TestSettings> options)
        {
            _logger = logger;
            _options = options;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            _logger.LogInformation(_options.CurrentValue?.Prop?.ToString() ?? "null");
            _logger.LogInformation(_options.CurrentValue?.Str ?? "null");

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}