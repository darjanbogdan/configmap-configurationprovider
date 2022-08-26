using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace ConfigMapConfigurationProvider.Tests;

public class ConfigMapDataConverterUnsafeTests
{
    private ConfigMapDataConverter _sut;
    
    public ConfigMapDataConverterUnsafeTests()
    {
        _sut = new ConfigMapDataConverter(safeConversion: false, new Lazy<ILogger>(Mock.Of<ILogger>()));
    }

    [Fact]
    public void Constructor_throws_when_logger_null()
    {
        var createInstance = () => new ConfigMapDataConverter(true, logger: null);

        createInstance.Should().ThrowExactly<ArgumentNullException>().WithMessage("*logger*");
    }

    [Fact]
    public void Convert_unsafe_handles_empty_dictionaries()
    {
        Dictionary<string, string> emptyConfigMapData = new();
        Dictionary<string, string> rawConfigMapData = new();

        var convertedData = _sut.ConvertData(emptyConfigMapData, rawConfigMapData);

        convertedData.Should().NotBeNull();
        convertedData.Count.Should().Be(0);
    }

    [Fact]
    public void Convert_unsafe_throws_on_null_current_config_map()
    {
        Dictionary<string, string> nullConfigMapData = null;
        Dictionary<string, string> rawConfigMapData = new();

        var convertFunc = () => _sut.ConvertData(nullConfigMapData, rawConfigMapData);

        convertFunc.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Convert_unsafe_throws_on_null_raw_config_map()
    {
        Dictionary<string, string> currentConfigMapData = new();
        Dictionary<string, string> nullConfigMapData = null;

        var convertFunc = () => _sut.ConvertData(currentConfigMapData, nullConfigMapData);

        convertFunc.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Convert_unsafe_update_succeeds()
    {
        Dictionary<string, string> emptyConfigMapData = new();
        Dictionary<string, string> rawConfigMapData = new()
        {
            ["Foo"] = "foo",
            ["Foo__Bar"] = "foo bar",
            ["Foo__Bar__Baz"] = "foo bar baz",
        };

        var convertedData = _sut.ConvertData(emptyConfigMapData, rawConfigMapData);

        convertedData.Should().NotBeNull();
        convertedData.Count.Should().Be(3);
        convertedData.ContainsKey("Foo").Should().BeTrue();
        convertedData.ContainsKey("Foo:Bar").Should().BeTrue();
        convertedData.ContainsKey("Foo:Bar:Baz").Should().BeTrue();
    }

    [Fact]
    public void Convert_unsafe_skips_value_type_mismatch_verification()
    {
        Dictionary<string, string> currentConfigMapData = new()
        {
            ["Foo"] = "true",
            ["Foo:Bar"] = "1",
            ["Foo:Bar:Baz"] = "foo bar baz"
        };

        Dictionary<string, string> rawConfigMapData = new()
        {
            ["Foo"] = "foo",
            ["Foo__Bar"] = "foo bar",
            ["Foo__Bar__Baz"] = "foo bar baz",
        };

        var convertedData = _sut.ConvertData(currentConfigMapData, rawConfigMapData);

        convertedData.Should().NotBeNull();
        convertedData.Count.Should().Be(3);
        convertedData.ContainsKey("Foo").Should().BeTrue();
        convertedData.ContainsKey("Foo:Bar").Should().BeTrue();
        convertedData.ContainsKey("Foo:Bar:Baz").Should().BeTrue();
    }
}
