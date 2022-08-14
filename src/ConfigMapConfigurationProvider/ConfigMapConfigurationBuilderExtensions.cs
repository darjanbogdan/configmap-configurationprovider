using ConfigMapConfigurationProvider;

namespace Microsoft.Extensions.Configuration;
public static class ConfigMapConfigurationBuilderExtensions
{
    public const string DefaultSettingsSection = "ConfigMapSettings";

    public static IConfigurationBuilder AddConfigMapConfiguration(
        this IConfigurationBuilder configurationBuilder, 
        string configMapSettingsSection = DefaultSettingsSection)
    {
        return configurationBuilder.Add(new ConfigMapConfigurationSource(configMapSettingsSection));
    }
}
