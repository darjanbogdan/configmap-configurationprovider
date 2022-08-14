using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMapConfigurationProvider
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="DefaultNamespace">Default namespace for cluster interactions</param>
    public record ConfigMapKubernetesSettings(string DefaultNamespace, string Name)
    {
        public ConfigMapKubernetesSettings()
            : this(DefaultNamespace: "default", Name: default)
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Kubernetes"></param>
    public record ConfigMapConfigurationProviderSettings(ConfigMapKubernetesSettings Kubernetes, TimeSpan? PollingInterval)
    {
        public ConfigMapConfigurationProviderSettings()
            : this(Kubernetes: new ConfigMapKubernetesSettings(), PollingInterval: default)
        {

        }
    }
}
