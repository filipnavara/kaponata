// <copyright file="KubernetesClientTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Operator.Kubernetes.Polyfill;
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
        /// The <see cref="KubernetesClient"/> class reads its configuration from the Kubernetes config file.
        /// </summary>
        [Fact]
        public void KubernetesClient_IsConfigured()
        {
            using (var client = new KubernetesClient(
                k8s.KubernetesClientConfiguration.BuildConfigFromConfigFile("Kubernetes/Polyfill/kubeconfig.yml")))
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
