// <copyright file="KubernetesServiceCollectionExtensionsTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Kaponata.Kubernetes.Tests
{
    /// <summary>
    /// Tests the <see cref="KubernetesServiceCollectionExtensions"/> class.
    /// </summary>
    public class KubernetesServiceCollectionExtensionsTests
    {
        /// <summary>
        /// <see cref="KubernetesServiceCollectionExtensions.AddKubernetes"/> validates its arguments.
        /// </summary>
        [Fact]
        public void AddKubernetes_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>("services", () => KubernetesServiceCollectionExtensions.AddKubernetes(null));
        }

        /// <summary>
        /// <see cref="KubernetesServiceCollectionExtensions.AddKubernetes"/> works correctly, and you can extract a <see cref="KubernetesClient"/>
        /// out of the configured service provider.
        /// </summary>
        [Fact]
        public void AddKubernetes_Works()
        {
            var collection = new ServiceCollection();
            collection.AddLogging();
            collection.AddKubernetes();

            using (var serviceProvider = collection.BuildServiceProvider())
            {
                var client = serviceProvider.GetRequiredService<KubernetesClient>();
                Assert.NotNull(client);
                Assert.Equal("default", client.Options.Namespace);
            }
        }

        /// <summary>
        /// <see cref="KubernetesServiceCollectionExtensions.AddKubernetes"/> works correctly, and uses the correct
        /// namespace.
        /// </summary>
        [Fact]
        public void AddKubernetes_WithNamespace_Works()
        {
            var collection = new ServiceCollection();
            collection.AddLogging();
            collection.AddKubernetes(@namespace: "my-namespace");

            using (var serviceProvider = collection.BuildServiceProvider())
            {
                var client = serviceProvider.GetRequiredService<KubernetesClient>();
                Assert.NotNull(client);
                Assert.Equal("my-namespace", client.Options.Namespace);
            }
        }

        /// <summary>
        /// <see cref="KubernetesServiceCollectionExtensions.AddKubernetes"/> validates its arguments.
        /// </summary>
        [Fact]
        public void AddKubernetes_WithNullNamespace_Throws()
        {
            var collection = new ServiceCollection();
            Assert.Throws<ArgumentNullException>("namespace", () => collection.AddKubernetes(@namespace: null));
        }
    }
}
