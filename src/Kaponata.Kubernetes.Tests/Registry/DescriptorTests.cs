// <copyright file="DescriptorTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Kubernetes.Registry;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Kubernetes.Tests.Registry
{
    /// <summary>
    /// Tests the <see cref="Descriptor"/> class.
    /// </summary>
    public class DescriptorTests
    {
        /// <summary>
        /// <see cref="Descriptor.CreateAsync(Stream, string, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateAsync_ValidatesArguments_Async()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => Descriptor.CreateAsync(null, string.Empty, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="Descriptor.CreateAsync(Stream, string, CancellationToken)"/> returns the correct values.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateAsync_Works_Async()
        {
            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes("Hello, World!")))
            {
                var descriptor = await Descriptor.CreateAsync(stream, "text/plain", default).ConfigureAwait(false);

                Assert.Equal("sha256:dffd6021bb2bd5b0af676290809ec3a53191dd81c7f70a4b28688a362182986f", descriptor.Digest);
                Assert.Equal("text/plain", descriptor.MediaType);
                Assert.Equal(13, descriptor.Size);
            }
        }
    }
}
