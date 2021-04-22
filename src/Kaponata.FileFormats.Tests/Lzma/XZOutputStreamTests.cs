// <copyright file="XZOutputStreamTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Packaging.Targets.IO;
using System;
using System.IO;
using System.Text;
using Xunit;

namespace Kaponata.FileFormats.Tests.Lzma
{
    /// <summary>
    /// Tests the <see cref="XZOutputStream"/> class.
    /// </summary>
    public class XZOutputStreamTests
    {
        /// <summary>
        /// The <see cref="XZOutputStream"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new XZOutputStream(null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new XZOutputStream(Stream.Null, 0));
        }

        /// <summary>
        /// The default properties of the <see cref="XZOutputStream"/> behave correctly.
        /// </summary>
        [Fact]
        public void SimpleProperties_Work()
        {
            using (Stream stream = new MemoryStream())
            using (XZOutputStream xzStream = new XZOutputStream(stream))
            {
                Assert.False(xzStream.CanRead);
                Assert.False(xzStream.CanSeek);
                Assert.True(xzStream.CanWrite);
                Assert.Throws<NotSupportedException>(() => xzStream.Length);
                Assert.Throws<NotSupportedException>(() => xzStream.Position);
                Assert.Throws<NotSupportedException>(() => xzStream.Position = 1);

                xzStream.Dispose();

                Assert.Throws<ObjectDisposedException>(() => xzStream.CanRead);
                Assert.Throws<ObjectDisposedException>(() => xzStream.CanSeek);
                Assert.Throws<ObjectDisposedException>(() => xzStream.CanWrite);
                Assert.Throws<ObjectDisposedException>(() => xzStream.Length);
                Assert.Throws<ObjectDisposedException>(() => xzStream.Position);
                Assert.Throws<ObjectDisposedException>(() => xzStream.Position = 1);
            }
        }

        /// <summary>
        /// <see cref="XZOutputStream.Flush"/> throws an exception.
        /// </summary>
        [Fact]
        public void Flush_Throws()
        {
            using (Stream stream = new MemoryStream())
            using (XZOutputStream xzStream = new XZOutputStream(stream))
            {
                Assert.Throws<NotSupportedException>(() => xzStream.Flush());

                xzStream.Dispose();
                Assert.Throws<ObjectDisposedException>(() => xzStream.Flush());
            }
        }

        /// <summary>
        /// <see cref="XZOutputStream.Seek"/> throws an exception.
        /// </summary>
        [Fact]
        public void Seek_Throws()
        {
            using (Stream stream = new MemoryStream())
            using (XZOutputStream xzStream = new XZOutputStream(stream))
            {
                Assert.Throws<NotSupportedException>(() => xzStream.Seek(0, SeekOrigin.Begin));

                xzStream.Dispose();
                Assert.Throws<ObjectDisposedException>(() => xzStream.Seek(0, SeekOrigin.Begin));
            }
        }

        /// <summary>
        /// <see cref="XZOutputStream.SetLength(long)"/> throws an exception.
        /// </summary>
        [Fact]
        public void SetLength_Throws()
        {
            using (Stream stream = new MemoryStream())
            using (XZOutputStream xzStream = new XZOutputStream(stream))
            {
                Assert.Throws<NotSupportedException>(() => xzStream.SetLength(0));

                xzStream.Dispose();
                Assert.Throws<ObjectDisposedException>(() => xzStream.SetLength(0));
            }
        }

        /// <summary>
        /// <see cref="XZOutputStream.Read(byte[], int, int)"/> throws an exception.
        /// </summary>
        [Fact]
        public void Read_Throws()
        {
            using (Stream stream = new MemoryStream())
            using (XZOutputStream xzStream = new XZOutputStream(stream))
            {
                Assert.Throws<NotSupportedException>(() => xzStream.Read(Array.Empty<byte>(), 0, 0));

                xzStream.Dispose();
                Assert.Throws<ObjectDisposedException>(() => xzStream.Read(Array.Empty<byte>(), 0, 0));
            }
        }

        /// <summary>
        /// <see cref="XZOutputStream.Write(byte[], int, int)"/> decompresses an .xz stream.
        /// </summary>
        [Fact]
        public void Write_Works()
        {
            using (Stream stream = new MemoryStream())
            using (XZOutputStream xzStream = new XZOutputStream(stream))
            {
                byte[] buffer = Encoding.UTF8.GetBytes("Hello, World!\n");

                xzStream.Write(buffer, 0, buffer.Length);

                xzStream.Dispose();
                Assert.Throws<ObjectDisposedException>(() => xzStream.Write(buffer, 0, 128));
            }
        }

        /// <summary>
        /// <see cref="XZOutputStream.Write(byte[], int, int)"/> does nothing when requested to
        /// write a zero-length value.
        /// </summary>
        [Fact]
        public void WriteEmpty_DoesNothing()
        {
            using (Stream stream = new MemoryStream())
            using (XZOutputStream xzStream = new XZOutputStream(stream))
            {
                xzStream.Write(Array.Empty<byte>(), 0, 0);

                Assert.Equal(0, stream.Position);
            }
        }

        /// <summary>
        /// <see cref="XZOutputStream.Encode(byte[], uint)"/> returns a valid XZ stream.
        /// </summary>
        [Fact]
        public void Encode_Works()
        {
            byte[] buffer = Encoding.UTF8.GetBytes("Hello, World!\n");
            var xz = XZOutputStream.Encode(buffer);

            Assert.Equal(0xfd, xz[0]);
            Assert.Equal((byte)'7', xz[1]);
            Assert.Equal((byte)'z', xz[2]);
            Assert.Equal((byte)'X', xz[3]);
            Assert.Equal((byte)'Z', xz[4]);
        }
    }
}
