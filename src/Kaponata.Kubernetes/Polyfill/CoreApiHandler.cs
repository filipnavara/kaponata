// <copyright file="CoreApiHandler.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Operator.Kubernetes.Polyfill
{
    /// <summary>
    /// A <see cref="DelegatingHandler"/> which intercepts requests to the <c>/apis/core/v1</c> or <c>/apis/v1</c> route
    /// and redirects them to <c>/api/v1</c>, so that "core" objects like <see cref="V1Pod"/> can be used
    /// by the <see cref="NamespacedKubernetesClient{T}"/>.
    /// </summary>
    public class CoreApiHandler : DelegatingHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreApiHandler"/> class.
        /// </summary>
        /// <param name="innerHandler">
        /// The inner handler.
        /// </param>
        public CoreApiHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        /// <inheritdoc/>
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.RequestUri.AbsolutePath.StartsWith("/apis/core/v1/"))
            {
                var builder = new UriBuilder(request.RequestUri);
                builder.Path = builder.Path.Replace("/apis/core/v1/", "/api/v1/");
                request.RequestUri = builder.Uri;
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
