// <copyright file="KubernetesClient.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Kaponata.Operator.Kubernetes.Polyfill;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Rest;
using Microsoft.Rest.Serialization;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks

namespace Kaponata.Operator.Kubernetes
{
    /// <summary>
    /// Performs high-level transactions (such as creating a Pod, waiting for a pod to enter a specific state,...)
    /// on top of the <see cref="IKubernetesProtocol"/>.
    /// </summary>
    public partial class KubernetesClient : IDisposable
    {
        private readonly IKubernetesProtocol protocol;
        private readonly ILogger<KubernetesClient> logger;
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="KubernetesClient"/> class.
        /// </summary>
        /// <param name="protocol">
        /// The protocol to use to communicate with the Kubernetes API server.
        /// </param>
        /// <param name="logger">
        /// The logger to use when logging.
        /// </param>
        /// <param name="loggerFactory">
        /// The logger factory to use when creating child objects.
        /// </param>
        public KubernetesClient(IKubernetesProtocol protocol, ILogger<KubernetesClient> logger, ILoggerFactory loggerFactory)
        {
            this.protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KubernetesClient"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor is intended for unit testing purposes only.
        /// </remarks>
        protected KubernetesClient()
        {
            this.logger = NullLogger<KubernetesClient>.Instance;
            this.loggerFactory = NullLoggerFactory.Instance;
        }

        private delegate Task<WatchExitReason> WatchObjectAsyncDelegate<T>(
            T value,
            Func<WatchEventType, T, Task<WatchResult>> onEvent,
            CancellationToken cancellationToken)
            where T : IKubernetesObject<V1ObjectMeta>;

        /// <inheritdoc/>
        public void Dispose()
        {
            this.protocol.Dispose();
        }

        private async Task<T> RunTaskAsync<T>(Task<T> task)
        {
            try
            {
                return await task.ConfigureAwait(false);
            }
            catch (HttpOperationException ex)
            when (ex.Response != null
                && ex.Response.Content != null
                && (ex.Response.StatusCode == HttpStatusCode.UnprocessableEntity || ex.Response.StatusCode == HttpStatusCode.Conflict))
            {
                // We should get a V1Status with a detailed error message, extract that error message.
                var status = SafeJsonConvert.DeserializeObject<V1Status>(ex.Response.Content);

                if (status == null || status.Kind != V1Status.KubeKind || status.ApiVersion != V1Status.KubeApiVersion || string.IsNullOrEmpty(status.Message))
                {
                    throw;
                }

                // Once https://github.com/kubernetes-client/csharp/pull/553 is merged, we can pass the inner exception, too.
                throw new KubernetesException(status);
            }
        }
    }
}
