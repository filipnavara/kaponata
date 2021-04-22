// <copyright file="LzmaExceptionTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Packaging.Targets.IO;
using Xunit;

namespace Kaponata.FileFormats.Tests.Lzma
{
    /// <summary>
    /// Tests the <see cref="LzmaException"/> class.
    /// </summary>
    public class LzmaExceptionTests
    {
        /// <summary>
        /// The default <see cref="LzmaException"/> constructor works.
        /// </summary>
        [Fact]
        public void DefaultConstructor_Works()
        {
            var ex = new LzmaException();
            Assert.Equal("An LZMA error occurred.", ex.Message);
        }

        /// <summary>
        /// THe <see cref="LzmaException.LzmaException(string)"/> constructor works.
        /// </summary>
        [Fact]
        public void StringConstructor_Works()
        {
            var ex = new LzmaException("test");
            Assert.Equal("test", ex.Message);
        }

        /// <summary>
        /// The <see cref="LzmaException.LzmaException(LzmaResult)"/> constructor works.
        /// </summary>
        /// <param name="result">
        /// The <see cref="LzmaResult"/> for which to throw the exception.
        /// </param>
        /// <param name="expectedMessage">
        /// The expected exception message.
        /// </param>
        [Theory]
        [InlineData(LzmaResult.MemError, "Memory allocation failed.")]
        [InlineData(LzmaResult.OptionsError, "Invalid or unsupported options.")]
        [InlineData(LzmaResult.FormatError, "The input is not in the .xz format.")]
        [InlineData(LzmaResult.DataError, "Compressed file is corrupt.")]
        [InlineData(LzmaResult.BufferError, "Compressed file is truncated or otherwise corrupt.")]
        [InlineData(LzmaResult.UnsupportedCheck, "An unknown LZMA error occurred: UnsupportedCheck.")]
        [InlineData((LzmaResult)99, "An unknown LZMA error occurred: 99.")]
        public void LzaResultConstructor_Works(LzmaResult result, string expectedMessage)
        {
            var ex = new LzmaException(result);
            Assert.Equal(result, (LzmaResult)ex.HResult);
            Assert.Equal(expectedMessage, ex.Message);
        }

        /// <summary>
        /// <see cref="LzmaException.ThrowOnError(LzmaResult)"/> does not throw on <see cref="LzmaResult.OK"/>
        /// values.
        /// </summary>
        [Fact]
        public void ThrowOnError_DoesNotThrowOnOK()
        {
            // This should not throw.
            LzmaException.ThrowOnError(LzmaResult.OK);
        }

        /// <summary>
        /// <see cref="LzmaException.ThrowOnError(LzmaResult)"/> does not throws on error values.
        /// </summary>
        [Fact]
        public void ThrowOnError_ThrowOnError()
        {
            var ex = Assert.Throws<LzmaException>(() => LzmaException.ThrowOnError(LzmaResult.MemError));
            Assert.Equal(LzmaResult.MemError, (LzmaResult)ex.HResult);
        }
    }
}
