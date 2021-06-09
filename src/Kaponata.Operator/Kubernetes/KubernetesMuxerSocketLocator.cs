// <copyright file="KubernetesMuxerSocketLocator.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Kaponata.iOS.Muxer;
using Kaponata.Kubernetes;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Operator.Kubernetes
{
    /// <summary>
    /// An implementation of the <see cref="MuxerSocketLocator"/> which connects to usbmuxd running in a Kubernetes pod,
    /// using port forwarding over the API server.
    /// </summary>
    public class KubernetesMuxerSocketLocator : MuxerSocketLocator
    {
        private readonly ILoggerFactory loggerFactory;
        private readonly IKubernetes kubernetes;
        private readonly V1Pod pod;

        /// <summary>
        /// Initializes a new instance of the <see cref="KubernetesMuxerSocketLocator"/> class.
        /// </summary>
        /// <param name="kubernetes">
        /// A connection to the Kubernetes cluster.
        /// </param>
        /// <param name="pod">
        /// The pod in which usbmuxd is running.
        /// </param>
        /// <param name="logger">
        /// A logger which is used when logging.
        /// </param>
        /// <param name="loggerFactory">
        /// A logger factory which an be used to create new loggers.
        /// </param>
        public KubernetesMuxerSocketLocator(
            IKubernetes kubernetes,
            V1Pod pod,
            ILogger<MuxerSocketLocator> logger,
            ILoggerFactory loggerFactory)
            : base(logger)
        {
            this.kubernetes = kubernetes ?? throw new ArgumentNullException(nameof(kubernetes));
            this.pod = pod ?? throw new ArgumentNullException(nameof(pod));
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        /// <inheritdoc/>
        public override async Task<Stream> ConnectToMuxerAsync(CancellationToken cancellationToken)
        {
            this.Logger.LogInformation("Connecting to port {port} on pod {pod}", DefaultMuxerPort, this.pod.Metadata.Name);

            var webSocket = await this.kubernetes.WebSocketNamespacedPodPortForwardAsync(
                this.pod.Metadata.Name,
                this.pod.Metadata.NamespaceProperty,
                new int[] { DefaultMuxerPort },
                cancellationToken: cancellationToken).ConfigureAwait(false);

            this.Logger.LogInformation("Connected to port {port} on pod {pod}", DefaultMuxerPort, this.pod.Metadata.Name);
            return new PortForwardStream(webSocket, this.loggerFactory.CreateLogger<PortForwardStream>());
        }

        /// <inheritdoc/>
        public override (Socket, EndPoint) GetMuxerSocket()
        {
            throw new NotSupportedException();
        }
    }
}
