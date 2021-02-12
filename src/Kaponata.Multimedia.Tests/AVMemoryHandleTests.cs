// <copyright file="AVMemoryHandleTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Multimedia.FFMpeg;
using Moq;
using System.Runtime.InteropServices;
using Xunit;

namespace Kaponata.Multimedia.Tests
{
    /// <summary>
    /// Tests the <see cref="AVMemoryHandle"/> class.
    /// </summary>
    public unsafe class AVMemoryHandleTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AVMemoryHandleTests"/> class.
        /// </summary>
        public AVMemoryHandleTests()
        {
            FFMpegClient.Initialize();
        }

        /// <summary>
        /// The <see cref="AVMemoryHandle.AVMemoryHandle(FFMpegClient, void*, bool)"/> initializes the instance.
        /// </summary>
        [Fact]
        public void Constuctor_InitializesInstance()
        {
            var ffmpegMock = new Mock<FFMpegClient>();

            var test = 0;
            ffmpegMock
                .Setup(c => c.FreeAVHandle(It.IsAny<SafeHandle>()))
                .Verifiable();

            using (var handle = new AVMemoryHandle(ffmpegMock.Object, &test, true))
            {
                Assert.Equal((int)&test, (int)handle.DangerousGetHandle().ToPointer());
            }

            ffmpegMock.Verify();
        }
    }
}
