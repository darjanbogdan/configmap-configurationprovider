using ConfigMapConfigurationProvider;
using ConfigMapConfigurationProvider.App.Controllers;

var builder = WebApplication.CreateBuilder(args);

// configuration
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Trace));
builder.Configuration.AddConfigMapConfiguration(loggerFactory);

// services
builder.Services
    .AddOptions<ConfigMapConfigurationProviderSettings>()
    .Bind<ConfigMapConfigurationProviderSettings>(builder.Configuration.GetSection(ConfigMapConfigurationBuilderExtensions.DefaultSettingsSection));

builder.Services
    .AddOptions<WeatherForecastSettings>()
    .Bind<WeatherForecastSettings>(builder.Configuration.GetSection("WeatherForecastSettings"));

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
