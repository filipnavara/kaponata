// <copyright file="DummyHandler.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Operator.Tests.Kubernetes.Polyfill
{
    /// <summary>
    /// A dummy <see cref="HttpMessageHandler"/> which can be used for testing purposes.
    /// </summary>
    internal class DummyHandler : HttpMessageHandler
    {
        /// <summary>
        /// Gets a list which contains the requests which have been recieved from the client.
        /// </summary>
        public List<HttpRequestMessage> Requests
        { get; } = new List<HttpRequestMessage>();

        /// <summary>
        /// Gets a queue which contains the responses to send to the client.
        /// </summary>
        public Queue<HttpResponseMessage> Responses
        { get; } = new Queue<HttpResponseMessage>();

        /// <inheritdoc/>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            this.Requests.Add(request);
            return Task.FromResult(this.Responses.Dequeue());
        }
    }
}
