using k8s;
using k8s.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace ConfigMapConfigurationProvider;

public class ConfigMapConfigurationProvider : ConfigurationProvider, IDisposable
{
    private readonly ConfigMapConfigurationProviderSettings _settings;
    private readonly Lazy<ILogger> _lazyLogger;

    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly ConfigMapDataConverter _dataConverter;
    private readonly Lazy<Kubernetes> _kubernetes;

    private bool disposedValue;

    public ConfigMapConfigurationProvider(ConfigMapConfigurationProviderSettings settings, ILoggerFactory loggerFactory)
    {
        _settings = settings;
        _lazyLogger = new Lazy<ILogger>(() => loggerFactory.CreateLogger<ConfigMapConfigurationProvider>());

        _cancellationTokenSource = new CancellationTokenSource();
        _dataConverter = new ConfigMapDataConverter(_settings.SafeUpdate, _lazyLogger);
        _kubernetes = new Lazy<Kubernetes>(() => new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig()));
    }

    public override void Load()
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await LoadAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _lazyLogger.Value.LogError(ex, "ConfigMap polling failed to start");
            }
        });
    }


    private async Task LoadAsync()
    {
        _lazyLogger.Value.LogTrace("ConfigMap {Name} changes polling initiated.", _settings.ConfigMapName);

        var configMapResponse = _kubernetes.Value.CoreV1.ListNamespacedConfigMapWithHttpMessagesAsync(
            namespaceParameter: _settings.DefaultNamespace,
            watch: true,
            fieldSelector: _settings.ConfigMapName is not null ? $"metadata.name={_settings.ConfigMapName}" : null
            );

        var configMapEnumerable = configMapResponse.WatchAsync<V1ConfigMap, V1ConfigMapList>(OnWatchError)
            .WithCancellation(_cancellationTokenSource.Token).ConfigureAwait(false);

        await foreach (var (eventType, configMap) in configMapEnumerable)
        {
            _lazyLogger.Value.LogTrace("Received ConfigMap {eventType} event with {Count} items.", eventType, configMap.Data.Count);

            Action<WatchEventType, IDictionary<string, string>> action = eventType switch
            {
                WatchEventType.Added or WatchEventType.Modified or WatchEventType.Deleted => ReloadConfigurationData,
                _ => LogUnhandledEvent
            };

            action(eventType, configMap.Data);
        }
    }

    private void ReloadConfigurationData(WatchEventType eventType, IDictionary<string, string> configMapData)
    {
        Data = _dataConverter.ConvertData(currentConfigMapData: Data, rawConfigMapData: configMapData);
        OnReload();
        _lazyLogger.Value.LogInformation("ConfigMap configuration reloaded after {eventType} event", eventType);
    }

    private void LogUnhandledEvent(WatchEventType eventType, IDictionary<string, string> configMapData)
    {
        _lazyLogger.Value.LogTrace("{eventType} event is not being handled.", eventType);
    }

    private void OnWatchError(Exception exception)
    {
        _lazyLogger.Value.LogError(exception, "ConfigMap polling error occurred, process needs to be restarted.");
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();

                _kubernetes?.Value?.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
