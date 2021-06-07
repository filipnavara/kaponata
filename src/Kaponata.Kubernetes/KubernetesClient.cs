// <copyright file="KubernetesClient.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Kaponata.Kubernetes.Models;
using Kaponata.Kubernetes.Polyfill;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Rest;
using Microsoft.Rest.Serialization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks

namespace Kaponata.Kubernetes
{
    /// <summary>
    /// Performs high-level transactions (such as creating a Pod, waiting for a pod to enter a specific state,...)
    /// on top of the <see cref="IKubernetesProtocol"/>.
    /// </summary>
    public partial class KubernetesClient : IDisposable
    {
        private readonly Dictionary<Type, KindMetadata> knownTypes = new Dictionary<Type, KindMetadata>();
        private readonly IKubernetesProtocol protocol;
        private readonly ILogger<KubernetesClient> logger;
        private readonly ILoggerFactory loggerFactory;
        private readonly IOptions<KubernetesOptions> options;

        /// <summary>
        /// Initializes a new instance of the <see cref="KubernetesClient"/> class.
        /// </summary>
        /// <param name="protocol">
        /// The protocol to use to communicate with the Kubernetes API server.
        /// </param>
        /// <param name="options">
        /// General options for the <see cref="KubernetesClient"/>.
        /// </param>
        /// <param name="logger">
        /// The logger to use when logging.
        /// </param>
        /// <param name="loggerFactory">
        /// The logger factory to use when creating child objects.
        /// </param>
        public KubernetesClient(IKubernetesProtocol protocol, IOptions<KubernetesOptions> options, ILogger<KubernetesClient> logger, ILoggerFactory loggerFactory)
        {
            this.protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            this.options = options ?? throw new ArgumentNullException(nameof(options));

            this.knownTypes.Add(typeof(MobileDevice), MobileDevice.KubeMetadata);
            this.knownTypes.Add(typeof(WebDriverSession), WebDriverSession.KubeMetadata);
            this.knownTypes.Add(typeof(V1Pod), new KindMetadata("core", V1Pod.KubeApiVersion, V1Pod.KubeKind, "pods"));
            this.knownTypes.Add(typeof(V1Service), new KindMetadata("core", V1Service.KubeApiVersion, V1Service.KubeKind, "services"));
            this.knownTypes.Add(typeof(V1Secret), new KindMetadata("core", V1Secret.KubeApiVersion, V1Secret.KubeKind, "secrets"));
            this.knownTypes.Add(typeof(V1Ingress), new KindMetadata(V1Ingress.KubeGroup, V1Ingress.KubeApiVersion, V1Ingress.KubeKind, "ingresses"));
            this.knownTypes.Add(typeof(V1ConfigMap), new KindMetadata("core", V1ConfigMap.KubeApiVersion, V1ConfigMap.KubeKind, "configmaps"));
        }

#nullable disable
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
            this.options = KubernetesOptions.Default;
        }
#nullable restore

        /// <summary>
        /// A delegate for a method which asynchronously watches a (namespaced) object.
        /// </summary>
        /// <typeparam name="T">
        /// The type of object to watch.
        /// </typeparam>
        /// <param name="value">
        /// The object to watch.
        /// </param>
        /// <param name="onEvent">
        /// An event handler which is invoked when the object changed.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous
        /// operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        public delegate Task<WatchExitReason> WatchObjectAsyncDelegate<T>(
            T value,
            WatchEventDelegate<T> onEvent,
            CancellationToken cancellationToken)
            where T : IKubernetesObject<V1ObjectMeta>;

        /// <summary>
        /// Gets a value indicating whether the code is currently running inside a pod hosted in a Kubernetes cluster.
        /// </summary>
        public virtual bool RunningInCluster => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST"));

        /// <summary>
        /// Gets the <see cref="KubernetesOptions"/> which configure this <see cref="KubernetesClient"/>.
        /// </summary>
        public KubernetesOptions Options => this.options.Value;

        /// <summary>
        /// Gets a <see cref="NamespacedKubernetesClient{T}"/> which allows you to interact with objects of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object being accessed.
        /// </typeparam>
        /// <returns>
        /// A <see cref="NamespacedKubernetesClient{T}"/> which allows you to operate on objects of <typeparamref name="T"/>.
        /// </returns>
        public virtual NamespacedKubernetesClient<T> GetClient<T>()
            where T : class, IKubernetesObject<V1ObjectMeta>, new()
        {
            return new NamespacedKubernetesClient<T>(
                this,
                this.knownTypes[typeof(T)],
                this.loggerFactory.CreateLogger<NamespacedKubernetesClient<T>>());
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            this.protocol.Dispose();
        }

        /// <summary>
        /// Runs a Kubernetes operation and extracts additional error messages from the <see cref="HttpOperationException"/>s when
        /// the operation fails.
        /// </summary>
        /// <typeparam name="T">
        /// The return type of the operation.
        /// </typeparam>
        /// <param name="task">
        /// The operation to execute.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the operation.
        /// </returns>
        public async Task<T> RunTaskAsync<T>(Task<T> task)
        {
            try
            {
                return await task.ConfigureAwait(false);
            }
            catch (HttpOperationException ex)
            when (ex.Response != null
                && ex.Response.Content != null
                && (ex.Response.StatusCode == HttpStatusCode.UnprocessableEntity || ex.Response.StatusCode == HttpStatusCode.Conflict || ex.Response.StatusCode == HttpStatusCode.BadRequest))
            {
                // We should get a V1Status with a detailed error message, extract that error message.
                var status = SafeJsonConvert.DeserializeObject<V1Status>(ex.Response.Content);

                if (status == null || status.Kind != V1Status.KubeKind || status.ApiVersion != V1Status.KubeApiVersion || string.IsNullOrEmpty(status.Message))
                {
                    throw;
                }

                throw new KubernetesException(status, ex);
            }
        }
    }
}
