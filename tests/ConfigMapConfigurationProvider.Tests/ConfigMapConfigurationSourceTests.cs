using AutoFixture;
using FluentAssertions;
using k8s;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using Xunit;

namespace ConfigMapConfigurationProvider.Tests;

public class ConfigMapConfigurationSourceTests
{
    [Fact]
    public void Constructor_throws_when_settings_null()
    {
        var createInstance = () => new ConfigMapConfigurationSource(configMapSettingsSection: null, optional: true);

        createInstance.Should().ThrowExactly<ArgumentNullException>().WithMessage("*configMapSettingsSection*");
    }

    [Fact]
    public void Constructor_throws_when_logger_null()
    {
        Lazy<Kubernetes> kubernetes = new Lazy<Kubernetes>(() => Mock.Of<Kubernetes>());

        var createInstance = () => new ConfigMapConfigurationSource("section", logger: null, kubernetes, optional: true);

        createInstance.Should().ThrowExactly<ArgumentNullException>().WithMessage("*logger*");
    }

    [Fact]
    public void Constructor_throws_when_kubernetes_null()
    {
        Lazy<ILogger> logger = new Lazy<ILogger>(() => Mock.Of<ILogger>());

        var createInstance = () => new ConfigMapConfigurationSource("section", logger, kubernetes: null, optional: true) ;

        createInstance.Should().ThrowExactly<ArgumentNullException>().WithMessage("*kubernetes*");
    }
}
