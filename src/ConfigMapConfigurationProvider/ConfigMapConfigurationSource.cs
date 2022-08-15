using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConfigMapConfigurationProvider;
public class ConfigMapConfigurationSource : IConfigurationSource
{
    private readonly ILoggerFactory? _loggerFactory;
    private readonly string _configMapSettingsSection;

    public ConfigMapConfigurationSource(string configMapSettingsSection, ILoggerFactory? loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _configMapSettingsSection = configMapSettingsSection;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        var baseConfiguration = builder.Build();
        var configMapSettings = baseConfiguration.GetSection(_configMapSettingsSection).Get<ConfigMapConfigurationProviderSettings>();

        return new ConfigMapConfigurationProvider(configMapSettings, _loggerFactory ?? new LoggerFactory());
    }
}