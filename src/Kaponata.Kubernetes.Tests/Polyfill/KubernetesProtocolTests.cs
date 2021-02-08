// <copyright file="KubernetesProtocolTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Exceptions;
using Kaponata.Kubernetes.Polyfill;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace Kaponata.Kubernetes.Tests.Polyfill
{
    /// <summary>
    /// Tests the <see cref="KubernetesProtocol"/> class.
    /// </summary>
    public partial class KubernetesProtocolTests
    {
        /// <summary>
        /// The <see cref="KubernetesProtocol"/> constructor should validate its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new KubernetesProtocol((HttpMessageHandler)null, NullLogger<KubernetesProtocol>.Instance, NullLoggerFactory.Instance));
            Assert.Throws<ArgumentNullException>("logger", () => new KubernetesProtocol(new SocketsHttpHandler(), null, NullLoggerFactory.Instance));
            Assert.Throws<ArgumentNullException>("loggerFactory", () => new KubernetesProtocol(new SocketsHttpHandler(), NullLogger<KubernetesProtocol>.Instance, null));

            Assert.Throws<KubeConfigException>(() => new KubernetesProtocol((KubernetesClientConfiguration)null, NullLogger<KubernetesProtocol>.Instance, NullLoggerFactory.Instance));
            Assert.Throws<ArgumentNullException>("logger", () => new KubernetesProtocol(KubernetesClientConfiguration.BuildConfigFromConfigFile("Polyfill/kubeconfig.yml"), null, NullLoggerFactory.Instance));
            Assert.Throws<ArgumentNullException>("loggerFactory", () => new KubernetesProtocol(KubernetesClientConfiguration.BuildConfigFromConfigFile("Polyfill/kubeconfig.yml"), NullLogger<KubernetesProtocol>.Instance, null));
        }

        /// <summary>
        /// The <see cref="KubernetesProtocol"/> class reads its configuration from the Kubernetes config file.
        /// </summary>
        [Fact]
        public void KubernetesClient_IsConfigured()
        {
            using (var protocol = new KubernetesProtocol(
                KubernetesClientConfiguration.BuildConfigFromConfigFile("Polyfill/kubeconfig.yml"),
                NullLogger<KubernetesProtocol>.Instance,
                NullLoggerFactory.Instance))
            {
                Assert.Collection(
                    protocol.HttpMessageHandlers,
                    (h) =>
                    {
                        var apiHandler = Assert.IsType<CoreApiHandler>(h);
                        Assert.NotNull(apiHandler.InnerHandler);
                    },
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

                Assert.Equal(new Uri("https://127.0.0.1:443"), protocol.BaseUri);
            }
        }
    }
}
