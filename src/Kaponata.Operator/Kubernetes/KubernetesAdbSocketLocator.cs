// <copyright file="KubernetesAdbSocketLocator.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.Adb;
using Kaponata.Kubernetes;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Operator.Kubernetes
{
    /// <summary>
    /// An implementation of the <see cref="AdbSocketLocator"/> which connects to adb running in a Kubernetes pod,
    /// using port forwarding over the API server.
    /// </summary>
    public class KubernetesAdbSocketLocator : AdbSocketLocator
    {
        private readonly KubernetesClient kubernetes;
        private readonly KubernetesAdbContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="KubernetesAdbSocketLocator"/> class.
        /// </summary>
        /// <param name="kubernetes">
        /// A connection to the Kubernetes cluster.
        /// </param>
        /// <param name="context">
        /// The pod in which adb is running.
        /// </param>
        public KubernetesAdbSocketLocator(
            KubernetesClient kubernetes,
            KubernetesAdbContext context)
        {
            this.kubernetes = kubernetes ?? throw new ArgumentNullException(nameof(kubernetes));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>
        public override Task<Stream> ConnectToAdbAsync(CancellationToken cancellationToken)
        {
            return this.kubernetes.ConnectToPodPortAsync(this.context.Pod, DefaultAdbPort, cancellationToken).AsTask();
        }

        /// <inheritdoc/>
        public override (Socket, EndPoint) GetAdbSocket()
        {
            throw new NotSupportedException();
        }
    }
}
