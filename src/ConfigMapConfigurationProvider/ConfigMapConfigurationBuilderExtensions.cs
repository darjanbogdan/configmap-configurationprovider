using ConfigMapConfigurationProvider;
using k8s;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Configuration;
/// <summary>
/// <see cref="IConfigurationBuilder"/> extensions for ConfigMap configuration provider
/// </summary>
public static class ConfigMapConfigurationBuilderExtensions
{
    /// <summary>
    /// The default settings section
    /// </summary>
    public const string DefaultSettingsSection = "ConfigMapSettings";

    /// <summary>
    /// Adds the configuration map configuration.
    /// </summary>
    /// <param name="configurationBuilder">The configuration builder.</param>
    /// <param name="configMapSettingsSection">The configuration map settings section.</param>
    /// <param name="optional">if set to <c>true</c> [optional].</param>
    /// <returns></returns>
    public static IConfigurationBuilder AddConfigMapConfiguration(
        this IConfigurationBuilder configurationBuilder,
        string configMapSettingsSection = DefaultSettingsSection,
        bool optional = false)
    {
        return configurationBuilder.Add(new ConfigMapConfigurationSource(configMapSettingsSection, optional));
    }

    /// <summary>
    /// Adds the configuration map configuration.
    /// </summary>
    /// <param name="configurationBuilder">The configuration builder.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="kubernetesFactory">The kubernetes factory.</param>
    /// <param name="configMapSettingsSection">The configuration map settings section.</param>
    /// <param name="optional">if set to <c>true</c> [optional].</param>
    /// <returns></returns>
    public static IConfigurationBuilder AddConfigMapConfiguration(
        this IConfigurationBuilder configurationBuilder,
        Func<ILogger> loggerFactory,
        Func<Kubernetes> kubernetesFactory,
        string configMapSettingsSection = DefaultSettingsSection,
        bool optional = false)
    {
        var lazyLogger = new Lazy<ILogger>(loggerFactory);
        var lazyKubernetes = new Lazy<Kubernetes>(kubernetesFactory);

        return configurationBuilder.Add(new ConfigMapConfigurationSource(configMapSettingsSection, lazyLogger, lazyKubernetes, optional));
    }
}
