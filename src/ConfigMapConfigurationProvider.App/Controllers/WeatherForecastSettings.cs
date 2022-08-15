namespace ConfigMapConfigurationProvider.App.Controllers
{
    public record WeatherForecastSettings(int? SampleSize, bool? IncludeFahrenheit)
    {
        public WeatherForecastSettings() : this(SampleSize: 5, IncludeFahrenheit: default)
        {

        }
    }
}