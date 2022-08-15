namespace ConfigMapConfigurationProvider;

/// <summary>
/// 
/// </summary>
public record ConfigMapConfigurationProviderSettings(string DefaultNamespace, string? ConfigMapName, bool SafeUpdate)
{
    public const string DefaultKubernetesNamespace = "default";

    public ConfigMapConfigurationProviderSettings()
        : this(DefaultKubernetesNamespace, ConfigMapName: null, SafeUpdate: false)
    {
    }
}
