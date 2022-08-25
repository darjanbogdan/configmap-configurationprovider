using k8s;
using k8s.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace ConfigMapConfigurationProvider;

/// <summary>
/// <see cref="ConfigurationProvider"/> for Kubernetes ConfigMap resource
/// </summary>
/// <seealso cref="Microsoft.Extensions.Configuration.ConfigurationProvider" />
/// <seealso cref="System.IDisposable" />
public class ConfigMapConfigurationProvider : ConfigurationProvider, IDisposable
{
    private readonly ConfigMapConfigurationProviderSettings _settings;
    private readonly Lazy<ILogger> _Logger;

    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly ConfigMapDataConverter _dataConverter;
    private readonly Lazy<Kubernetes> _kubernetes;

    private bool disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigMapConfigurationProvider"/> class.
    /// </summary>
    /// <param name="settings">The settings.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="kubernetes">The kubernetes client instance.</param>
    public ConfigMapConfigurationProvider(ConfigMapConfigurationProviderSettings settings, Lazy<ILogger> logger, Lazy<Kubernetes> kubernetes)
    {
        _settings = settings;
        _Logger = logger;
        _kubernetes = kubernetes;

        _cancellationTokenSource = new CancellationTokenSource();
        _dataConverter = new ConfigMapDataConverter(_settings.SafeUpdate, _Logger);
    }


    /// <inheritdoc/>
    public override void Load()
    {
        try
        {
            LoadAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex) when (_settings.IsOptional)
        {
            _Logger.Value.LogError(ex, "ConfigMap polling failed to start, skipping as it's optional.");
        }
    }

    private async Task LoadAsync()
    {
        _Logger.Value.LogTrace("ConfigMap {Name} changes polling initiated.", _settings.ConfigMapName);

        var configMapResponse = _kubernetes.Value.CoreV1.ListNamespacedConfigMapWithHttpMessagesAsync(
            namespaceParameter: _settings.DefaultNamespace,
            watch: true,
            fieldSelector: _settings.ConfigMapName is not null ? $"metadata.name={_settings.ConfigMapName}" : null,
            allowWatchBookmarks: true
            );

        var configMapEnumerable = configMapResponse.WatchAsync<V1ConfigMap, V1ConfigMapList>(OnWatchError)
            .WithCancellation(_cancellationTokenSource.Token).ConfigureAwait(false);

        await foreach (var (eventType, configMap) in configMapEnumerable)
        {
            _Logger.Value.LogTrace("Received ConfigMap {eventType} event with {Count} items.", eventType, configMap.Data.Count);

            if (eventType is WatchEventType.Added or WatchEventType.Modified or WatchEventType.Deleted)
            {
                ReloadConfigurationData(eventType, configMap.Data);
            }
            else
            {
                LogUnhandledEvent(eventType);
            }
        }
    }

    private void ReloadConfigurationData(WatchEventType eventType, IDictionary<string, string> configMapData)
    {
        Data = _dataConverter.ConvertData(currentConfigMapData: Data, rawConfigMapData: configMapData);
        OnReload();
        _Logger.Value.LogInformation("ConfigMap configuration reloaded after {eventType} event", eventType);
    }

    private void LogUnhandledEvent(WatchEventType eventType)
    {
        _Logger.Value.LogTrace("{eventType} event is not being handled.", eventType);
    }

    private void OnWatchError(Exception exception)
    {
        _Logger.Value.LogError(exception, "ConfigMap polling error occurred, process needs to be restarted.");
    }

    /// <summary>
    /// Disposes the the instance.
    /// </summary>
    /// <param name="disposing">if set to <c>true</c> [disposing].</param>
    /// <returns></returns>
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

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <returns></returns>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
