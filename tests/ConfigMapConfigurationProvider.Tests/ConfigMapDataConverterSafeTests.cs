using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace ConfigMapConfigurationProvider.Tests;

public class ConfigMapDataConverterSafeTests
{
    private ConfigMapDataConverter _sut;
    private ILogger _logger;

    public ConfigMapDataConverterSafeTests()
    {
        _logger = Mock.Of<ILogger>();

        _sut = new ConfigMapDataConverter(safeConversion: true, new Lazy<ILogger>(() => _logger)); ;
    }

    [Fact]
    public void Convert_safe_handles_empty_dictionaries()
    {
        Dictionary<string, string> emptyConfigMapData = new();
        Dictionary<string, string> rawConfigMapData = new();

        var convertedData = _sut.ConvertData(emptyConfigMapData, rawConfigMapData);

        convertedData.Should().NotBeNull();
        convertedData.Count.Should().Be(0);
    }

    [Fact]
    public void Convert_safe_throws_on_null_current_config_map()
    {
        Dictionary<string, string> rawConfigMapData = new();

        var convertFunc = () => _sut.ConvertData(currentConfigMapData: null, rawConfigMapData);

        convertFunc.Should().ThrowExactly<ArgumentNullException>().WithMessage("*currentConfigMapData*"); ;
    }

    [Fact]
    public void Convert_safe_throws_on_null_raw_config_map()
    {
        Dictionary<string, string> currentConfigMapData = new();
        
        var convertFunc = () => _sut.ConvertData(currentConfigMapData, rawConfigMapData: null);

        convertFunc.Should().ThrowExactly<ArgumentNullException>().WithMessage("*rawConfigMapData*");
    }

    [Fact]
    public void Convert_safe_update_succeeds_when_current_config_empty()
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
    public void Convert_safe_update_succeeds()
    {
        Dictionary<string, string> currentConfigMapData = new()
        { 
            ["Foo"] = "bar",
            ["Foo:Bar"] = "foo bar",
        };
        Dictionary<string, string> rawConfigMapData = new()
        {
            ["Foo"] = "baz",
            ["Foo__Bar"] = "foo baz",
        };

        var convertedData = _sut.ConvertData(currentConfigMapData, rawConfigMapData);

        convertedData.Should().NotBeNull();
        convertedData.Count.Should().Be(2);
        
        convertedData.ContainsKey("Foo").Should().BeTrue();
        convertedData["Foo"].Should().Be("baz");

        convertedData.ContainsKey("Foo:Bar").Should().BeTrue();
        convertedData["Foo:Bar"].Should().Be("foo baz");
    }

    [Fact]
    public void Convert_safe_update_skips_update_on_type_mismatch()
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
            ["Foo__Bar__Baz"] = "foo bar baz updated",
        };

        var convertedData = _sut.ConvertData(currentConfigMapData, rawConfigMapData);

        convertedData.Should().NotBeNull();
        convertedData.Count.Should().Be(3);
        
        convertedData.ContainsKey("Foo").Should().BeTrue();
        convertedData["Foo"].Should().Be("true");
        
        convertedData.ContainsKey("Foo:Bar").Should().BeTrue();
        convertedData["Foo:Bar"].Should().Be("1");

        convertedData.ContainsKey("Foo:Bar:Baz").Should().BeTrue();
        convertedData["Foo:Bar:Baz"].Should().Be("foo bar baz updated");
    }

    [Fact]
    public void Convert_supports_primitive_type_compatibility_check()
    {
        Dictionary<string, string> currentConfigMapData = new()
        {
            ["FooBoolean"] = "true",
            ["FooString"] = "foo",
            ["FooInt32"] = $"{Int32.MaxValue}",
            ["FooInt64"] = $"{Int64.MaxValue}",
            ["FooSingle"] = $"{Single.MaxValue}",
            ["FooDouble"] = $"{Double.MaxValue}",
            ["FooTimeSpan"] = $"{TimeSpan.MaxValue}",
            ["FooTimeOnly"] = $"{TimeOnly.MaxValue}",
            ["FooDateOnly"] = $"{DateOnly.MaxValue}",
            ["FooDateTime"] = $"{DateTime.MaxValue}",
        };
        Dictionary<string, string> rawConfigMapData = new()
        {
            ["FooBoolean"] = "false",
            ["FooString"] = "bar",
            ["FooInt32"] = $"{Int32.MinValue}",
            ["FooInt64"] = $"{Int64.MinValue}",
            ["FooSingle"] = $"{Single.Epsilon}",
            ["FooDouble"] = $"{Double.Epsilon}",
            ["FooTimeSpan"] = $"{TimeSpan.MinValue}",
            ["FooTimeOnly"] = $"{TimeOnly.MinValue}",
            ["FooDateOnly"] = $"{DateOnly.MinValue}",
            ["FooDateTime"] = $"{DateTime.MinValue}",
        };

        var convertedData = _sut.ConvertData(currentConfigMapData, rawConfigMapData);

        convertedData.Should().NotBeNull();
        convertedData.Count.Should().Be(10);

        convertedData.ContainsKey("FooBoolean").Should().BeTrue();
        convertedData["FooBoolean"].Should().Be("false");

        convertedData.ContainsKey("FooString").Should().BeTrue();
        convertedData["FooString"].Should().Be("bar");

        convertedData.ContainsKey("FooInt32").Should().BeTrue();
        convertedData["FooInt32"].Should().Be($"{Int32.MinValue}");

        convertedData.ContainsKey("FooInt64").Should().BeTrue();
        convertedData["FooInt64"].Should().Be($"{Int64.MinValue}");

        convertedData.ContainsKey("FooSingle").Should().BeTrue();
        convertedData["FooSingle"].Should().Be($"{Single.Epsilon}");

        convertedData.ContainsKey("FooDouble").Should().BeTrue();
        convertedData["FooDouble"].Should().Be($"{Double.Epsilon}");

        convertedData.ContainsKey("FooTimeSpan").Should().BeTrue();
        convertedData["FooTimeSpan"].Should().Be($"{TimeSpan.MinValue}");

        convertedData.ContainsKey("FooTimeOnly").Should().BeTrue();
        convertedData["FooTimeOnly"].Should().Be($"{TimeOnly.MinValue}");

        convertedData.ContainsKey("FooDateOnly").Should().BeTrue();
        convertedData["FooDateOnly"].Should().Be($"{DateOnly.MinValue}");

        convertedData.ContainsKey("FooDateTime").Should().BeTrue();
        convertedData["FooDateTime"].Should().Be($"{DateTime.MinValue}");
    }

    [Fact]
    public void Convert_adds_new_key_value_when_existing_key_not_found_or_null()
    {
        Dictionary<string, string> currentConfigMapData = new()
        {
            ["Foo"] = "bar",
            ["Foo:Bar"] = null,
        };
        Dictionary<string, string> rawConfigMapData = new()
        {
            ["Foo"] = "bar",
            ["Foo__Bar"] = "foo bar",
        };

        var convertedData = _sut.ConvertData(currentConfigMapData, rawConfigMapData);

        convertedData.Should().NotBeNull();
        convertedData.Count.Should().Be(2);

        convertedData.ContainsKey("Foo").Should().BeTrue();
        convertedData["Foo"].Should().Be("bar");

        convertedData.ContainsKey("Foo:Bar").Should().BeTrue();
        convertedData["Foo:Bar"].Should().Be("foo bar");
    }

    [Fact]
    public void Convert_skips_keys_with_same_value()
    {
        Dictionary<string, string> currentConfigMapData = new()
        {
            ["Foo"] = "bar",
        };
        Dictionary<string, string> rawConfigMapData = new()
        {
            ["Foo"] = "bar",
        };

        var convertedData = _sut.ConvertData(currentConfigMapData, rawConfigMapData);

        convertedData.Should().NotBeNull();
        convertedData.Count.Should().Be(1);

        convertedData.ContainsKey("Foo").Should().BeTrue();
        convertedData["Foo"].Should().Be("bar");
    }

    [Fact]
    public void Convert_logs_warning_when_key_values_are_incompatible()
    {
        Dictionary<string, string> currentConfigMapData = new()
        {
            ["Foo"] = "true"
        };

        Dictionary<string, string> rawConfigMapData = new()
        {
            ["Foo"] = "foo"
        };

        var convertedData = _sut.ConvertData(currentConfigMapData, rawConfigMapData);

        convertedData.Should().NotBeNull();
        convertedData.Count.Should().Be(1);

        convertedData.ContainsKey("Foo").Should().BeTrue();
        convertedData["Foo"].Should().Be("true");

        Mock.Get(_logger)
            .Verify(l => l.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning), 
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ), Times.Once());
    }
}
