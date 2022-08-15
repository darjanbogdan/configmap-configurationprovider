using ConfigMapConfigurationProvider;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Configuration;
public static class ConfigMapConfigurationBuilderExtensions
{
    public const string DefaultSettingsSection = "ConfigMapSettings";

    public static IConfigurationBuilder AddConfigMapConfiguration(
        this IConfigurationBuilder configurationBuilder,
        ILoggerFactory? loggerFactory = null,
        string configMapSettingsSection = DefaultSettingsSection)
    {
        return configurationBuilder.Add(new ConfigMapConfigurationSource(configMapSettingsSection, loggerFactory));
    }
}
