using ConfigMapConfigurationProvider;
using ConfigMapConfigurationProvider.App.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddConfigMapConfiguration();

// Add services to the container.

builder.Services
    .AddOptions<ConfigMapConfigurationProviderSettings>()
    .Bind<ConfigMapConfigurationProviderSettings>(builder.Configuration.GetSection(ConfigMapConfigurationBuilderExtensions.DefaultSettingsSection));

builder.Services
    .AddOptions<TestSettings>()
    .Bind<TestSettings>(builder.Configuration.GetSection("TestSettings"));

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
