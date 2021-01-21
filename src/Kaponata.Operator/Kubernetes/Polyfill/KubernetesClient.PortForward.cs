// <copyright file="KubernetesClient.PortForward.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Operator.Kubernetes.Polyfill
{
    /// <summary>
    /// Implements the <see cref="KubernetesClient.ConnectToPodPortAsync(V1Pod, int, CancellationToken)"/> method.
    /// </summary>
    public partial class KubernetesClient
    {
        /// <inheritdoc/>
        public async Task<Stream> ConnectToPodPortAsync(V1Pod pod, int port, CancellationToken cancellationToken)
        {
            if (pod == null)
            {
                throw new ArgumentNullException(nameof(pod));
            }

            this.logger.LogInformation("Connecting to port {port} on pod {pod}", port, pod.Metadata.Name);

            var webSocket = await this.WebSocketNamespacedPodPortForwardAsync(
                pod.Metadata.Name,
                pod.Metadata.NamespaceProperty,
                new int[] { port },
                cancellationToken: cancellationToken).ConfigureAwait(false);

            this.logger.LogInformation("Connected to port {port} on pod {pod}", port, pod.Metadata.Name);
            return new PortForwardStream(webSocket, this.loggerFactory.CreateLogger<PortForwardStream>());
        }
    }
}