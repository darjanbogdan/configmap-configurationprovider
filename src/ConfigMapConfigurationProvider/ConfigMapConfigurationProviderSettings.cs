namespace ConfigMapConfigurationProvider;

/// <summary>
/// Settings used to poll the right ConfigMap
/// </summary>
/// <param name="DefaultNamespace">Default Kubernetes namespace</param>
/// <param name="ConfigMapName">Name of Kubernetes ConfigMap resource</param>
/// <param name="SafeUpdate">Flag to switch safe configuration updates</param>
public record ConfigMapConfigurationProviderSettings(string DefaultNamespace, string? ConfigMapName, bool SafeUpdate)
{
    /// <summary>
    /// The default kubernetes namespace
    /// </summary>
    public const string DefaultKubernetesNamespace = "default";
    private bool _optional;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigMapConfigurationProviderSettings"/> class.
    /// </summary>
    public ConfigMapConfigurationProviderSettings()
        : this(DefaultKubernetesNamespace, ConfigMapName: null, SafeUpdate: false)
    {
    }

    /// <summary>Sets whether configuration provider is optional.</summary>
    /// <param name="optional">if set to <c>true</c> [optional].</param>
    public void SetOptional(bool optional)
    {
        _optional = optional;
    }

    /// <summary>
    /// Gets a value indicating whether the configuration provider is optional.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is optional; otherwise, <c>false</c>.
    /// </value>
    public bool IsOptional => _optional;
}
