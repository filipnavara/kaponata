// <copyright file="KubernetesClientTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Exceptions;
using Kaponata.Operator.Kubernetes.Polyfill;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace Kaponata.Operator.Tests.Kubernetes.Polyfill
{
    /// <summary>
    /// Tests the <see cref="KubernetesClient"/> class.
    /// </summary>
    public partial class KubernetesClientTests
    {
        /// <summary>
        /// The <see cref="KubernetesClient"/> constructor should validate its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new KubernetesClient((HttpMessageHandler)null, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance));
            Assert.Throws<ArgumentNullException>("logger", () => new KubernetesClient(new SocketsHttpHandler(), null, NullLoggerFactory.Instance));
            Assert.Throws<ArgumentNullException>("loggerFactory", () => new KubernetesClient(new SocketsHttpHandler(), NullLogger<KubernetesClient>.Instance, null));

            Assert.Throws<KubeConfigException>(() => new KubernetesClient((KubernetesClientConfiguration)null, NullLogger<KubernetesClient>.Instance, NullLoggerFactory.Instance));
            Assert.Throws<ArgumentNullException>("logger", () => new KubernetesClient(KubernetesClientConfiguration.BuildConfigFromConfigFile("Kubernetes/Polyfill/kubeconfig.yml"), null, NullLoggerFactory.Instance));
            Assert.Throws<ArgumentNullException>("loggerFactory", () => new KubernetesClient(KubernetesClientConfiguration.BuildConfigFromConfigFile("Kubernetes/Polyfill/kubeconfig.yml"), NullLogger<KubernetesClient>.Instance, null));
        }

        /// <summary>
        /// The <see cref="KubernetesClient"/> class reads its configuration from the Kubernetes config file.
        /// </summary>
        [Fact]
        public void KubernetesClient_IsConfigured()
        {
            using (var client = new KubernetesClient(
                KubernetesClientConfiguration.BuildConfigFromConfigFile("Kubernetes/Polyfill/kubeconfig.yml"),
                NullLogger<KubernetesClient>.Instance,
                NullLoggerFactory.Instance))
            {
                Assert.Collection(
                    client.HttpMessageHandlers,
                    (h) =>
                    {
                        var watchHandler = Assert.IsType<WatchHandler>(h);
                        Assert.NotNull(watchHandler.InnerHandler);
                    },
                    (h) =>
                    {
                        var handler = Assert.IsType<HttpClientHandler>(h);
                        Assert.Collection(
                            handler.ClientCertificates.OfType<X509Certificate2>(),
                            c =>
                            {
                                Assert.Equal("C2E33CEADA8CA7416367A0CA639EA155CC920319", c.Thumbprint);
                            });
                    });

                Assert.Equal(new Uri("https://127.0.0.1:443"), client.BaseUri);
            }
        }
    }
}
