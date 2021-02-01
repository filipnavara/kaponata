// <copyright file="ShellStreamTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.Adb;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Android.Tests.Adb
{
    /// <summary>
    /// Tests the <see cref="ShellStream"/> class.
    /// </summary>
    public class ShellStreamTests
    {
        /// <summary>
        /// The <see cref="ShellStream.ShellStream(Stream, bool)"/> validates the arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new ShellStream(null, false));
            using var stream = new MemoryStream();
            Assert.Throws<ArgumentOutOfRangeException>(() => new ShellStream(stream, false));
        }

        /// <summary>
        /// The <see cref="ShellStream.ShellStream(Stream, bool)"/> constructor initializes the instance.
        /// </summary>
        [Fact]
        public void Constructor_Initialization()
        {
            using (MemoryStream stream = new MemoryStream())
            using (ShellStream shellStream = new ShellStream(stream, false))
            {
                Assert.Equal(stream, shellStream.Inner);
                Assert.True(shellStream.CanRead);
                Assert.False(shellStream.CanSeek);
                Assert.False(shellStream.CanWrite);
            }
        }

        /// <summary>
        /// The <see cref="ShellStream.ReadAsync(byte[], int, int, System.Threading.CancellationToken)"/> reads the stream and eliminates the CR character.
        /// </summary>
        /// <param name="input">
        /// The input data.
        /// </param>
        /// <param name="expected">
        /// The expected output of the read operation.
        /// </param>
        /// <param name="bufferSize">
        /// The size of the buffer to use.
        /// </param>
        [Theory]
        [InlineData(
            "\r\nHello!",
            new byte[] { (byte)'\n', (byte)'H', (byte)'e', (byte)'l', (byte)'l', (byte)'o', (byte)'!' },
            1)]
        [InlineData(
            "\r\n1\r\n2\r\n3\r\n",
            new byte[] { (byte)'\n', (byte)'1', (byte)'\n', (byte)'2', (byte)'\n', (byte)'3', (byte)'\n' },
            1)]
        [InlineData(
            "\r\nH\ra",
            new byte[] { (byte)'\n', (byte)'H', (byte)'\r', (byte)'a' },
            1)]
        [InlineData(
            "\r\nHello!",
            new byte[] { (byte)'\n', (byte)'H', (byte)'e', (byte)'l', (byte)'l', (byte)'o', (byte)'!' },
            2)]
        [InlineData(
            "\r\n1\r\n2\r\n3\r\n",
            new byte[] { (byte)'\n', (byte)'1', (byte)'\n', (byte)'2', (byte)'\n', (byte)'3', (byte)'\n' },
            2)]
        [InlineData(
            "\r\nH\ra",
            new byte[] { (byte)'\n', (byte)'H', (byte)'\r', (byte)'a' },
            2)]
        [InlineData(
            "\r\nHello!",
            new byte[] { (byte)'\n', (byte)'H', (byte)'e', (byte)'l', (byte)'l', (byte)'o', (byte)'!' },
            100)]
        [InlineData(
            "\r\n1\r\n2\r\n3\r\n",
            new byte[] { (byte)'\n', (byte)'1', (byte)'\n', (byte)'2', (byte)'\n', (byte)'3', (byte)'\n' },
            100)]
        [InlineData(
            "\r\nH\ra",
            new byte[] { (byte)'\n', (byte)'H', (byte)'\r', (byte)'a' },
            100)]
        public void Read_ReadStream(string input, byte[] expected, int bufferSize)
        {
            using var stream = GetStream(input);
            var buffer = new byte[bufferSize];
            var result = new byte[expected.Length];
            int totalRead = 0;
            int read = 0;
            while ((read = stream.Read(buffer)) > 0)
            {
                buffer.AsMemory(0, read).CopyTo(result.AsMemory(totalRead));
                totalRead = totalRead + read;
            }

            Assert.Equal(expected.Length, totalRead);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// The <see cref="ShellStream.ReadAsync(byte[], int, int, System.Threading.CancellationToken)"/> reads the stream and eliminates the CR character.
        /// </summary>
        /// <param name="input">
        /// The input data.
        /// </param>
        /// <param name="expected">
        /// The expected output of the read operation.
        /// </param>
        /// <param name="bufferSize">
        /// The size of the buffer to use.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Theory]
        [InlineData(
            "\r\nHello!",
            new byte[] { (byte)'\n', (byte)'H', (byte)'e', (byte)'l', (byte)'l', (byte)'o', (byte)'!' },
            1)]
        [InlineData(
            "\r\n1\r\n2\r\n3\r\n",
            new byte[] { (byte)'\n', (byte)'1', (byte)'\n', (byte)'2', (byte)'\n', (byte)'3', (byte)'\n' },
            1)]
        [InlineData(
            "\r\nH\ra",
            new byte[] { (byte)'\n', (byte)'H', (byte)'\r', (byte)'a' },
            1)]
        [InlineData(
            "\r\nHello!",
            new byte[] { (byte)'\n', (byte)'H', (byte)'e', (byte)'l', (byte)'l', (byte)'o', (byte)'!' },
            2)]
        [InlineData(
            "\r\n1\r\n2\r\n3\r\n",
            new byte[] { (byte)'\n', (byte)'1', (byte)'\n', (byte)'2', (byte)'\n', (byte)'3', (byte)'\n' },
            2)]
        [InlineData(
            "\r\nH\ra",
            new byte[] { (byte)'\n', (byte)'H', (byte)'\r', (byte)'a' },
            2)]
        [InlineData(
            "\r\nHello!",
            new byte[] { (byte)'\n', (byte)'H', (byte)'e', (byte)'l', (byte)'l', (byte)'o', (byte)'!' },
            100)]
        [InlineData(
            "\r\n1\r\n2\r\n3\r\n",
            new byte[] { (byte)'\n', (byte)'1', (byte)'\n', (byte)'2', (byte)'\n', (byte)'3', (byte)'\n' },
            100)]
        [InlineData(
            "\r\nH\ra",
            new byte[] { (byte)'\n', (byte)'H', (byte)'\r', (byte)'a' },
            100)]
        public async Task ReadAsync_ReadStream_Async(string input, byte[] expected, int bufferSize)
        {
            using var stream = GetStream(input);
            var buffer = new byte[bufferSize];
            var result = new byte[expected.Length];
            int totalRead = 0;
            int read = 0;
            while ((read = await stream.ReadAsync(buffer.AsMemory(0, bufferSize)).ConfigureAwait(false)) > 0)
            {
                buffer.AsMemory(0, read).CopyTo(result.AsMemory(totalRead));
                totalRead += read;
            }

            Assert.Equal(expected.Length, totalRead);
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// The <see cref="Stream.ReadByte()"/> reads the stream and eliminates the CR character.
        /// </summary>
        /// <param name="input">
        /// The input data.
        /// </param>
        /// <param name="expected">
        /// The expected output of the read operation.
        /// </param>
        [Theory]
        [InlineData(
            "\r\nHello!",
            new byte[] { (byte)'\n', (byte)'H', (byte)'e', (byte)'l', (byte)'l', (byte)'o', (byte)'!' })]
        [InlineData(
            "\r\n1\r\n2\r\n3\r\n",
            new byte[] { (byte)'\n', (byte)'1', (byte)'\n', (byte)'2', (byte)'\n', (byte)'3', (byte)'\n' })]
        [InlineData(
            "\r\nH\ra",
            new byte[] { (byte)'\n', (byte)'H', (byte)'\r', (byte)'a' })]
        public void ReadByte_ReadsByte(string input, byte[] expected)
        {
            using var stream = GetStream(input);
            int received;
            var index = 0;
            while ((received = stream.ReadByte()) != -1)
            {
                Assert.Equal(expected[index], received);
                index++;
            }
        }

        /// <summary>
        /// The <see cref="StreamReader.ReadToEnd"/> reads the stream and eliminates the CR character.
        /// </summary>
        /// <param name="input">
        /// The input data.
        /// </param>
        /// <param name="expected">
        /// The expected output of the read operation.
        /// </param>
        [Theory]
        [InlineData(
            "\r\nHello!",
            "\nHello!")]
        [InlineData(
            "\r\n1\r\n2\r\n3\r\n",
            "\n1\n2\n3\n")]
        [InlineData(
            "\r\nH\ra",
            "\nH\ra")]
        public void ReadToEnd_ReadsAll(string input, string expected)
        {
            using var stream = GetStream(input);
            using var reader = new StreamReader(stream);

            Assert.Equal(expected, reader.ReadToEnd());
        }

        /// <summary>
        /// The <see cref="StreamReader.ReadToEndAsync"/> reads the stream and eliminates the CR character.
        /// </summary>
        /// <param name="input">
        /// The input data.
        /// </param>
        /// <param name="expected">
        /// The expected output of the read operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Theory]
        [InlineData(
            "\r\nHello!",
            "\nHello!")]
        [InlineData(
            "\r\n1\r\n2\r\n3\r\n",
            "\n1\n2\n3\n")]
        [InlineData(
            "\r\nH\ra",
            "\nH\ra")]
        public async Task ReadToEndAsync_ReadsAll_Async(string input, string expected)
        {
            using var stream = GetStream(input);
            using var reader = new StreamReader(stream);

            Assert.Equal(expected, await reader.ReadToEndAsync().ConfigureAwait(false));
        }

        private static ShellStream GetStream(string text)
        {
            byte[] data = Encoding.ASCII.GetBytes(text);
            return new ShellStream(new MemoryStream(data), true);
        }
    }
}
