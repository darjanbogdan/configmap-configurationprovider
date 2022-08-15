using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMapConfigurationProvider;

public sealed class ConfigMapDataConverter
{
    private readonly bool _safeConversion;
    private readonly Lazy<ILogger> _logger;

    public ConfigMapDataConverter(bool safeConversion, Lazy<ILogger> logger)
    {
        _safeConversion = safeConversion;
        _logger = logger;
    }

    public IDictionary<string, string> ConvertData(IDictionary<string, string> currentConfigMapData, IDictionary<string, string> rawConfigMapData)
        => _safeConversion ? ConvertDataKeysSafe(currentConfigMapData, rawConfigMapData) : ConvertDataKeysUnsafe(rawConfigMapData);

    private IDictionary<string, string> ConvertDataKeysSafe(IDictionary<string, string> currentConfigMapData, IDictionary<string, string> configMapData)
    {
        Dictionary<string, string> convertedConfigMapData = new(configMapData.Count);
        
        foreach (var newItem in configMapData)
        {
            (string key, string value) = ConvertItemSafely(currentConfigMapData, newItem);
            convertedConfigMapData.Add(key, value);
        }

        return convertedConfigMapData;
    }

    private (string key, string value) ConvertItemSafely(IDictionary<string, string> currentConfigMapData, KeyValuePair<string, string> newItem)
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

    private object ParseToPrimitiveTypeOrDefault(string value)
    {
        return
            TryParse<Boolean>(Boolean.Parse, value)
            ?? TryParse<Int32>(Int32.Parse, value)
            ?? TryParse<Int64>(Int64.Parse, value)
            ?? TryParse<Single>(Single.Parse, value)
            ?? TryParse<Double>(Double.Parse, value)
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

    private bool ValueTypesCompatible(string newValue, object currentValue)
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

    private static IDictionary<string, string> ConvertDataKeysUnsafe(IDictionary<string, string> rawConfigMapData)
        => rawConfigMapData.ToDictionary(
            i => ConvertKey(i.Key),
            i => i.Value);

    private static string ConvertKey(string key) => key.Replace("__", ConfigurationPath.KeyDelimiter);
}
