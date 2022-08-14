using k8s;
using Microsoft.Extensions.Configuration;

namespace ConfigMapConfigurationProvider;
public class ConfigMapConfigurationSource : IConfigurationSource
{
    private readonly string _configMapSettingsSection;

    public ConfigMapConfigurationSource(string configMapSettingsSection)
    {
        _configMapSettingsSection = configMapSettingsSection;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        var baseConfiguration = builder.Build();
        var configMapSettings = baseConfiguration.GetSection(_configMapSettingsSection).Get<ConfigMapConfigurationProviderSettings>();
        
        return new ConfigMapConfigurationProvider(configMapSettings);
    }
}