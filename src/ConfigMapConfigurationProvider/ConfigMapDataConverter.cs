using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace ConfigMapConfigurationProvider;

/// <summary>
/// Converter of the ConfigMap data
/// </summary>
public sealed class ConfigMapDataConverter
{
    private readonly bool _safeConversion;
    private readonly Lazy<ILogger> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigMapDataConverter"/> class.
    /// </summary>
    /// <param name="safeConversion">if set to <c>true</c> [safe conversion].</param>
    /// <param name="logger">The logger.</param>
    public ConfigMapDataConverter(bool safeConversion, Lazy<ILogger> logger)
    {
        _safeConversion = safeConversion;
        _logger = logger;
    }

    /// <summary>
    /// Converts the ConfigMap data.
    /// </summary>
    /// <param name="currentConfigMapData">The current configuration map data.</param>
    /// <param name="rawConfigMapData">The raw configuration map data.</param>
    /// <returns></returns>
    public IDictionary<string, string> ConvertData(IDictionary<string, string> currentConfigMapData, IDictionary<string, string> rawConfigMapData)
        => _safeConversion 
            ? ConvertDataKeysSafe(currentConfigMapData, rawConfigMapData) 
            : ConvertDataKeysUnsafe(rawConfigMapData);

    private IDictionary<string, string> ConvertDataKeysSafe(IDictionary<string, string> currentConfigMapData, IDictionary<string, string> configMapData)
    {
        Dictionary<string, string> convertedConfigMapData = new(configMapData.Count);
        
        foreach (var newItem in configMapData)
        {
            (string key, string value) = ConvertItemSafe(currentConfigMapData, newItem);
            convertedConfigMapData.Add(key, value);
        }

        return convertedConfigMapData;
    }

    private static IDictionary<string, string> ConvertDataKeysUnsafe(IDictionary<string, string> rawConfigMapData)
        => rawConfigMapData.ToDictionary(
            i => ConvertKey(i.Key),
            i => i.Value);

    private static string ConvertKey(string key) => key.Replace("__", ConfigurationPath.KeyDelimiter);

    private (string key, string value) ConvertItemSafe(IDictionary<string, string> currentConfigMapData, KeyValuePair<string, string> newItem)
    {
        string convertedKey = ConvertKey(newItem.Key);

        if (currentConfigMapData.TryGetValue(convertedKey, out string? currentValue) is false || currentValue is null)
        {
            return (convertedKey, newItem.Value); // return 'new' if 'current' doesn't exist or null
        }

        if (currentValue == newItem.Value)
        {
            return (convertedKey, currentValue); // skip parsing if values are the same
        }

        object? parsedCurrentValue = ParseToPrimitiveTypeOrDefault(currentValue);

        if (parsedCurrentValue is string)
        {
            return (convertedKey, newItem.Value); // return 'new' in case 'current' is string
        }

        if (ValueTypesCompatible(newItem.Value, parsedCurrentValue))
        {
            return (convertedKey, newItem.Value); // return 'new' in case 'current' and 'new' types are compatible
        }

        _logger.Value.LogWarning(
            "ConfigMap item {key} with the new value {newValue} is incompatible with the current value {currentValue}, update skipped.", 
            convertedKey, newItem.Value, currentValue);

        return (convertedKey, currentValue); // return 'current' in case 'new' incompatible
    }

    private static object ParseToPrimitiveTypeOrDefault(string value)
    {
        return
            TryParse(Boolean.Parse, value)
            ?? TryParse(Int32.Parse, value)
            ?? TryParse(Int64.Parse, value)
            ?? TryParse(Single.Parse, value)
            ?? TryParse(Double.Parse, value)
            ?? TryParse(TimeSpan.Parse, value)
            ?? TryParse(TimeOnly.Parse, value)
            ?? TryParse(DateOnly.Parse, value)
            ?? TryParse(DateTime.Parse, value)
            ?? value as object;

        T? TryParse<T>(Func<string, T> parse, string value) where T : struct
        {
            try
            {
                return parse(value);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    private static bool ValueTypesCompatible(string newValue, object currentValue)
    {
        try
        {
            var converter = TypeDescriptor.GetConverter(currentValue.GetType());
            return converter.IsValid(newValue);
        }
        catch (Exception)
        {
            return false;
        }
    }
}
