// <copyright file="KubernetesProtocol.Watch.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Rest.Serialization;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Operator.Kubernetes.Polyfill
{
    /// <summary>
    /// Implements the watch methods on the <see cref="KubernetesProtocol"/> class.
    /// </summary>
    public partial class KubernetesProtocol
    {
        /// <inheritdoc/>
        public async Task<WatchExitReason> WatchPodAsync(
            V1Pod pod,
            Func<WatchEventType, V1Pod, Task<WatchResult>> eventHandler,
            CancellationToken cancellationToken)
        {
            if (pod == null)
            {
                throw new ArgumentNullException(nameof(pod));
            }

            if (eventHandler == null)
            {
                throw new ArgumentNullException(nameof(eventHandler));
            }

            using (var response = await this.ListNamespacedPodWithHttpMessagesAsync(
                pod.Metadata.NamespaceProperty,
                fieldSelector: $"metadata.name={pod.Metadata.Name}",
                resourceVersion: pod.Metadata.ResourceVersion,
                watch: true,
                cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                using (var watchContent = (WatchHttpContent)response.Response.Content)
                using (var content = watchContent.OriginalContent)
                using (var stream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
                using (var reader = new StreamReader(stream))
                {
                    cancellationToken.Register(watchContent.Dispose);

                    string line;

                    // ReadLineAsync will return null when we've reached the end of the stream.
                    try
                    {
                        while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                        {
                            var genericEvent =
                                SafeJsonConvert.DeserializeObject<Watcher<KubernetesObject>.WatchEvent>(line);

                            if (genericEvent.Object.Kind == "Status")
                            {
                                var statusEvent = SafeJsonConvert.DeserializeObject<Watcher<V1Status>.WatchEvent>(line);
                                this.logger.LogInformation("Stopped watching pod {pod} because of a status event with payload {status}", pod.Metadata.Name, statusEvent.Object);
                                throw new KubernetesException(statusEvent.Object);
                            }
                            else
                            {
                                var @event = SafeJsonConvert.DeserializeObject<Watcher<V1Pod>.WatchEvent>(line);
                                this.logger.LogDebug("Got an {event} event for pod {pod}", @event.Type, pod.Metadata.Name);

                                if (await eventHandler(@event.Type, @event.Object).ConfigureAwait(false) == WatchResult.Stop)
                                {
                                    this.logger.LogInformation("Stopped watching pod {pod} because the client requested to stop watching.", pod.Metadata.Name);
                                    return WatchExitReason.ClientDisconnected;
                                }
                            }
                        }
                    }
                    catch (Exception ex) when (cancellationToken.IsCancellationRequested)
                    {
                        this.logger.LogInformation("Stopped watching {pod} because a cancellation request was received.", pod.Metadata.Name);
                        throw new TaskCanceledException("The watch operation was cancelled.", ex);
                    }

                    this.logger.LogInformation("Stopped watching {pod} because the server closed the connection.", pod.Metadata.Name);
                    return WatchExitReason.ServerDisconnected;
                }
            }
        }
    }
}
