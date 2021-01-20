// <copyright file="WatchHandler.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Operator.Kubernetes.Polyfill
{
    /// <summary>
    /// A <see cref="DelegatingHandler"/> which intercepts GET requests with the <c>watch=true</c> parameter in the querystring,
    /// and returns a <see cref="WatchHttpContent"/> object for those requests.
    /// </summary>
    public class WatchHandler : DelegatingHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WatchHandler"/> class.
        /// </summary>
        /// <param name="innerHandler">
        /// The inner handler.
        /// </param>
        public WatchHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        /// <inheritdoc/>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var originResponse = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            // all watches are GETs, so we can ignore others
            if (originResponse.IsSuccessStatusCode && request.Method == HttpMethod.Get)
            {
                var query = request.RequestUri.Query;
                var index = query.IndexOf("watch=true", StringComparison.InvariantCulture);
                if (index > 0 && (query[index - 1] == '&' || query[index - 1] == '?'))
                {
                    originResponse.Content = new WatchHttpContent(originResponse.Content);
                }
            }

            return originResponse;
        }
    }
}
