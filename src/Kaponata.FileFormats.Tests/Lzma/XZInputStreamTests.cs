// <copyright file="XZInputStreamTests.cs" company="Quamotion bv">
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
    /// Tests the <see cref="XZInputStream"/> class.
    /// </summary>
    public class XZInputStreamTests
    {
        /// <summary>
        /// The <see cref="XZInputStream"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new XZInputStream(null));
        }

        /// <summary>
        /// The default properties of the <see cref="XZInputStream"/> behave correctly.
        /// </summary>
        [Fact]
        public void SimpleProperties_Work()
        {
            using (Stream stream = File.OpenRead("Lzma/hello.xz"))
            using (XZInputStream xzStream = new XZInputStream(stream))
            {
                Assert.True(xzStream.CanRead);
                Assert.False(xzStream.CanSeek);
                Assert.False(xzStream.CanWrite);

                // Call this property twice, the stream will return a cached value
                // when possible
                Assert.Equal(14, xzStream.Length);
                Assert.Equal(14, xzStream.Length);

                Assert.Equal(0, xzStream.Position);
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
        /// <see cref="XZInputStream.Flush"/> throws an exception.
        /// </summary>
        [Fact]
        public void Flush_Throws()
        {
            using (Stream stream = File.OpenRead("Lzma/hello.xz"))
            using (XZInputStream xzStream = new XZInputStream(stream))
            {
                Assert.Throws<NotSupportedException>(() => xzStream.Flush());

                xzStream.Dispose();
                Assert.Throws<ObjectDisposedException>(() => xzStream.Flush());
            }
        }

        /// <summary>
        /// <see cref="XZInputStream.Seek"/> throws an exception.
        /// </summary>
        [Fact]
        public void Seek_Throws()
        {
            using (Stream stream = File.OpenRead("Lzma/hello.xz"))
            using (XZInputStream xzStream = new XZInputStream(stream))
            {
                Assert.Throws<NotSupportedException>(() => xzStream.Seek(0, SeekOrigin.Begin));

                xzStream.Dispose();
                Assert.Throws<ObjectDisposedException>(() => xzStream.Seek(0, SeekOrigin.Begin));
            }
        }

        /// <summary>
        /// <see cref="XZInputStream.SetLength(long)"/> throws an exception.
        /// </summary>
        [Fact]
        public void SetLength_Throws()
        {
            using (Stream stream = File.OpenRead("Lzma/hello.xz"))
            using (XZInputStream xzStream = new XZInputStream(stream))
            {
                Assert.Throws<NotSupportedException>(() => xzStream.SetLength(0));

                xzStream.Dispose();
                Assert.Throws<ObjectDisposedException>(() => xzStream.SetLength(0));
            }
        }

        /// <summary>
        /// <see cref="XZInputStream.Write(byte[], int, int)"/> throws an exception.
        /// </summary>
        [Fact]
        public void Write_Throws()
        {
            using (Stream stream = File.OpenRead("Lzma/hello.xz"))
            using (XZInputStream xzStream = new XZInputStream(stream))
            {
                Assert.Throws<NotSupportedException>(() => xzStream.Write(Array.Empty<byte>(), 0, 0));

                xzStream.Dispose();
                Assert.Throws<ObjectDisposedException>(() => xzStream.Write(Array.Empty<byte>(), 0, 0));
            }
        }

        /// <summary>
        /// <see cref="XZInputStream.Read(byte[], int, int)"/> decompresses an .xz stream.
        /// </summary>
        [Fact]
        public void Read_Works()
        {
            using (Stream stream = File.OpenRead("Lzma/hello.xz"))
            using (XZInputStream xzStream = new XZInputStream(stream))
            {
                byte[] buffer = new byte[128];
                Assert.Equal(14, xzStream.Read(buffer, 0, 128));
                Assert.Equal(0, xzStream.Read(buffer, 0, 128));

                Assert.Equal("Hello, World!\n", Encoding.UTF8.GetString(buffer, 0, 14));

                xzStream.Dispose();

                Assert.Throws<ObjectDisposedException>(() => xzStream.Read(buffer, 0, 128));
            }
        }
    }
}
