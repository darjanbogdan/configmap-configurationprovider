using ConfigMapConfigurationProvider;
using ConfigMapConfigurationProvider.App.Controllers;
using k8s;

var builder = WebApplication.CreateBuilder(args);

// configuration
var loggerFactory = () => LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Trace)).CreateLogger<ConfigMapConfigurationProvider.ConfigMapConfigurationProvider>();
var kubernetesFactory = () => new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig());
//builder.Configuration.AddConfigMapConfiguration(loggerFactory, kubernetesFactory);
builder.Configuration.AddConfigMapConfiguration(optional: true);

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
