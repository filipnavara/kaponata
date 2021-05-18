// <copyright file="ImageRegistryClientFactory.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Kubernetes.Registry
{
    /// <summary>
    /// A factory class which can create <see cref="ImageRegistryClient"/> classes.
    /// </summary>
    public class ImageRegistryClientFactory
    {
        private readonly KubernetesClient client;
        private readonly ImageRegistryClientConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageRegistryClientFactory"/> class.
        /// </summary>
        /// <param name="client">
        /// A <see cref="KubernetesClient"/> which provides access to the Kubernetes cluster.
        /// </param>
        /// <param name="configuration">
        /// A <see cref="ImageRegistryClientConfiguration"/> which represents the configuration for the registry.
        /// </param>
        public ImageRegistryClientFactory(KubernetesClient client, ImageRegistryClientConfiguration configuration)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageRegistryClientFactory"/> class. Intended for mocking purposes only.
        /// </summary>
#nullable disable
        protected ImageRegistryClientFactory()
        {
        }
#nullable restore

        /// <summary>
        /// Asynchrously creates a <see cref="ImageRegistryClient"/>.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation and which, when completed, returns a <see cref="ImageRegistryClient"/>.
        /// </returns>
        public virtual async Task<ImageRegistryClient> CreateAsync(CancellationToken cancellationToken)
        {
            if (this.client.RunningInCluster)
            {
                // We're running inside a Kubernetes cluster, and can directly connect to the service.
                return new ImageRegistryClient(
                    new HttpClient()
                    {
                        BaseAddress = new Uri($"http://{this.configuration.ServiceName}:{this.configuration.Port}"),
                    });
            }
            else
            {
                // We're not running inside a Kubernetes cluster (e.g. a dev/test setup), and we can't directly connect to the service
                // through its IP address or DNS name. Select an individual pod backing the service and use port forwarding instead.
                var serviceClient = this.client.GetClient<V1Service>();
                var service = await serviceClient.TryReadAsync(this.configuration.ServiceName, cancellationToken).ConfigureAwait(false);

                if (service == null)
                {
                    throw new ImageRegistryException($"The service {this.configuration.ServiceName} does not exist");
                }

                var selector = LabelSelector.Create(service.Spec.Selector);

                var pods = await this.client.ListPodAsync(labelSelector: selector, cancellationToken: cancellationToken).ConfigureAwait(false);

                if (pods.Items.Count == 0)
                {
                    throw new ImageRegistryException($"No pods could be fond for service {this.configuration.ServiceName}.");
                }

                var pod = pods.Items[0];

                var httpClient = this.client.CreatePodHttpClient(pod, this.configuration.Port);
                return new ImageRegistryClient(httpClient);
            }
        }
    }
}
