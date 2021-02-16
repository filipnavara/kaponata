// <copyright file="AVRationalTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Multimedia.FFMpeg;
using Xunit;
using NativeAVRational = FFmpeg.AutoGen.AVRational;

namespace Kaponata.Multimedia.Tests
{
    /// <summary>
    /// Tests the <see cref="AVRational"/> class.
    /// </summary>
    public unsafe class AVRationalTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AVRationalTests"/> class.
        /// </summary>
        public AVRationalTests()
        {
            FFMpegClient.Initialize();
        }

        /// <summary>
        /// The <see cref="AVRational.Numerator"/> returns the native numerator.
        /// </summary>
        [Fact]
        public void Numerator_ReturnsNativeNumerator()
        {
            var nativeRational = new NativeAVRational
            {
                num = 4,
            };

            var rational = new AVRational(&nativeRational);
            Assert.Equal(4, rational.Numerator);
        }

        /// <summary>
        /// The <see cref="AVRational.Denominator"/> returns the native denominator.
        /// </summary>
        [Fact]
        public void Denominator_ReturnsNativeDenominator()
        {
            var nativeRational = new NativeAVRational
            {
                den = 100,
            };

            var rational = new AVRational(&nativeRational);
            Assert.Equal(100, rational.Denominator);
        }
    }
}
