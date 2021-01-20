// <copyright file="WatchHttpContentTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Operator.Kubernetes.Polyfill;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Operator.Tests.Kubernetes.Polyfill
{
    /// <summary>
    /// Tests the <see cref="WatchHttpContent"/> class.
    /// </summary>
    public class WatchHttpContentTests
    {
        /// <summary>
        /// The <see cref="WatchHttpContent"/> constructor validates the arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>("inner", () => new WatchHttpContent(null));
        }

        /// <summary>
        /// The <see cref="WatchHttpContent"/> class keeps track of the inner content passed
        /// via the constructor.
        /// </summary>
        [Fact]
        public void WatchHttpContent_TracksInnerContent()
        {
            var inner = new StringContent(string.Empty);
            var content = new WatchHttpContent(inner);

            Assert.Same(inner, content.OriginalContent);
        }

        /// <summary>
        /// The <see cref="WatchHttpContent"/> class returns an empty stream.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task WatchHttpContent_ReturnsEmptyStream_Async()
        {
            var content = new WatchHttpContent(new StringContent(string.Empty));

            using (var stream = await content.ReadAsStreamAsync())
            {
                Assert.Equal(0, stream.Length);
            }
        }

        /// <summary>
        /// Disposing of a <see cref="WatchHttpContent"/> object disposes of its inner object.
        /// </summary>
        [Fact]
        public void WatchHttpContent_DisposeInner()
        {
            var inner = new DummyContent();
            var content = new WatchHttpContent(inner);

            Assert.False(inner.Disposed);
            content.Dispose();
            Assert.True(inner.Disposed);
        }

        private class DummyContent : HttpContent
        {
            public bool Disposed { get; private set; }

            protected override Task SerializeToStreamAsync(System.IO.Stream stream, TransportContext context)
            {
                throw new NotImplementedException();
            }

            protected override bool TryComputeLength(out long length)
            {
                throw new NotImplementedException();
            }

            protected override void Dispose(bool disposing)
            {
                this.Disposed = true;
            }
        }
    }
}
