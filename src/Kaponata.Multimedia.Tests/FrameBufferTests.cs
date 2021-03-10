// <copyright file="FrameBufferTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Multimedia.FFmpeg;
using System;
using Xunit;

namespace Kaponata.Multimedia.Tests
{
    /// <summary>
    /// Tests the <see cref="FrameBuffer"/> class.
    /// </summary>
    public class FrameBufferTests
    {
        /// <summary>
        /// The <see cref="FrameBuffer.CopyFramebuffer(Memory{byte})"/> throws when the frame buffer is disposed.
        /// </summary>
        [Fact]
        public void CopyFramebuffer_ThrowsOnDisposed()
        {
            var frameBuffer = new FrameBuffer();
            frameBuffer.Dispose();

            Assert.Throws<ObjectDisposedException>(() => frameBuffer.CopyFramebuffer(new byte[] { }));
        }

        /// <summary>
        /// The <see cref="FrameBuffer.CopyFramebuffer(Memory{byte})"/> throws when the buffer is null.
        /// </summary>
        [Fact]
        public void CopyFramebuffer_ThrowsNullBuffer()
        {
            var frameBuffer = new FrameBuffer();

            Assert.Throws<InvalidOperationException>(() => frameBuffer.CopyFramebuffer(new byte[] { }));
        }
    }
}
