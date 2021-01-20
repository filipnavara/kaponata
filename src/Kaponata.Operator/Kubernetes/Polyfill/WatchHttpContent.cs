// <copyright file="WatchHttpContent.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Kaponata.Operator.Kubernetes.Polyfill
{
    /// <summary>
    /// The <see cref="HttpContent"/> which is returned by the <see cref="WatchHandler"/>. The <see cref="WatchHttpContent"/>
    /// returns an empty content (intended to be used by the REST API), and the original content is stored in the
    /// <see cref="OriginalContent"/> property (for use by the Watch API).
    /// </summary>
    public class WatchHttpContent : HttpContent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WatchHttpContent"/> class.
        /// </summary>
        /// <param name="inner">
        /// The inner content being wrapped.
        /// </param>
        public WatchHttpContent(HttpContent inner)
        {
            this.OriginalContent = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        /// <summary>
        /// Gets the original content being wrapepd.
        /// </summary>
        public HttpContent OriginalContent
        { get; private set; }

        /// <inheritdoc/>
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return true;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            this.OriginalContent.Dispose();

            base.Dispose(disposing);
        }
    }
}
