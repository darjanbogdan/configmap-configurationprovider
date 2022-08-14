using k8s;
using k8s.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace ConfigMapConfigurationProvider;

public class ConfigMapConfigurationProvider : ConfigurationProvider, IDisposable
{
    private readonly ConfigMapConfigurationProviderSettings _settings;

    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Kubernetes _kubernetes;
    private Task _configMapPollingTask;

    private bool disposedValue;

    public ConfigMapConfigurationProvider(ConfigMapConfigurationProviderSettings settings)
    {
        _settings = settings;
        
        _cancellationTokenSource = new CancellationTokenSource();
        _kubernetes = new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig());
    }

    public override void Load()
    {
        _ = Task.Run(async () => 
        {
            try
            {
                await LoadAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw; // fails to start polling config map
            }
        });
    }
    

    private async Task LoadAsync()
    {
        var configMapResponse = _kubernetes.CoreV1.ListNamespacedConfigMapWithHttpMessagesAsync(
            namespaceParameter: _settings.Kubernetes.DefaultNamespace, 
            watch: true, 
            fieldSelector: $"metadata.name={_settings.Kubernetes.Name}"
            );

        var configMapEnumerable = configMapResponse.WatchAsync<V1ConfigMap, V1ConfigMapList>(OnWatchError)
            .WithCancellation(_cancellationTokenSource.Token).ConfigureAwait(false);
        
        await foreach (var (eventType, configMapItem) in configMapEnumerable)
        {
            switch (eventType)
            {
                case WatchEventType.Added:
                case WatchEventType.Modified:
                    Data = ConvertData(configMapItem.Data);
                    OnReload();
                    continue;
                default:
                    Console.WriteLine($"{eventType} event type is not being handled.");
                    continue;
            }
        }

        //if (_configMapPollingTask != null && _settings.PollingInterval.HasValue)
        //{
        //    _configMapPollingTask = PollConfigMapChangesAsync();
        //}
    }

    private IDictionary<string, string> ConvertData(IDictionary<string, string> configMapData)
        => configMapData.ToDictionary(
            k => k.Key.Replace("__", ConfigurationPath.KeyDelimiter),
            v => v.Value);

    private void OnWatchError(Exception exception)
    { 
        // handle exception, log or surface
    }

    //private async Task PollConfigMapChangesAsync()
    //{
    //    while (_cancellationTokenSource.IsCancellationRequested is false)
    //    {
    //        await Task.Delay(_settings.PollingInterval.GetValueOrDefault(), _cancellationTokenSource.Token).ConfigureAwait(false);
    //        try
    //        {
    //            await LoadAsync().ConfigureAwait(false);
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine(ex.ToString());
    //        }
    //    }
    //}

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();

                _kubernetes?.Dispose();
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
