using k8s;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConfigMapConfigurationProvider;
/// <summary>
/// <see cref="IConfigurationSection"/> for Kubernetes ConfigMap resource
/// </summary>
/// <seealso cref="Microsoft.Extensions.Configuration.IConfigurationSource" />
public class ConfigMapConfigurationSource : IConfigurationSource
{
    private readonly string _configMapSettingsSection;
        
    private readonly Lazy<ILogger>? _logger;
    private readonly Lazy<Kubernetes>? _kubernetes;

    private readonly bool _optional;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigMapConfigurationSource" /> class.
    /// </summary>
    /// <param name="configMapSettingsSection">The configuration map settings section.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="kubernetes">The kubernetes.</param>
    /// <param name="optional">if set to <c>true</c> [optional].</param>
    /// <exception cref="System.ArgumentNullException">configMapSettingsSection or logger or kubernetes</exception>
    public ConfigMapConfigurationSource(string configMapSettingsSection, Lazy<ILogger> logger, Lazy<Kubernetes> kubernetes, bool optional)
        : this(configMapSettingsSection, optional)
    {
        _configMapSettingsSection = configMapSettingsSection ?? throw new ArgumentNullException(nameof(configMapSettingsSection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _kubernetes = kubernetes ?? throw new ArgumentNullException(nameof(kubernetes));
        _optional = optional;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigMapConfigurationSource"/> class.
    /// </summary>
    /// <param name="configMapSettingsSection">The configuration map settings section.</param>
    /// <param name="optional">if set to <c>true</c> [optional].</param>
    /// <exception cref="System.ArgumentNullException">configMapSettingsSection</exception>
    public ConfigMapConfigurationSource(string configMapSettingsSection, bool optional)
    {
        _configMapSettingsSection = configMapSettingsSection ?? throw new ArgumentNullException(nameof(configMapSettingsSection));
        _logger = new Lazy<ILogger>(() => new LoggerFactory().CreateLogger<ConfigMapConfigurationProvider>()); //dummy
        _kubernetes = new Lazy<Kubernetes>(() => new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig()));
        _optional = optional;
    }

    /// <summary>
    /// Builds the <see cref="T:Microsoft.Extensions.Configuration.IConfigurationProvider" /> for this source.
    /// </summary>
    /// <param name="builder">The <see cref="T:Microsoft.Extensions.Configuration.IConfigurationBuilder" />.</param>
    /// <returns>
    /// An <see cref="T:Microsoft.Extensions.Configuration.IConfigurationProvider" />
    /// </returns>
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        var baseConfiguration = builder.Build();
        
        var configMapSettings = baseConfiguration.GetSection(_configMapSettingsSection).Get<ConfigMapConfigurationProviderSettings>();

        if (configMapSettings is null)
        {
            throw new ArgumentException($"Section '{_configMapSettingsSection}' couldn't be bounded to '{nameof(ConfigMapConfigurationProviderSettings)}' instance.");
        }
        
        configMapSettings.SetOptional(_optional);

        return new ConfigMapConfigurationProvider(configMapSettings, _logger!, _kubernetes!);
    }
}