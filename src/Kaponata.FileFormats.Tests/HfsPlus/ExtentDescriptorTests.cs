// <copyright file="ExtentDescriptorTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.HfsPlus;
using System;
using System.Buffers.Binary;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// Tests the <see cref="ExtentDescriptor"/> class.
    /// </summary>
    public class ExtentDescriptorTests
    {
        /// <summary>
        /// <see cref="ExtentDescriptor.ReadFrom(byte[], int)"/> works correctly.
        /// </summary>
        [Fact]
        public void ReadFrom_Works()
        {
            var descriptor = new ExtentDescriptor();
            descriptor.ReadFrom(Convert.FromBase64String("AAAAAQAAAAE="), 0);

            Assert.Equal(1u, descriptor.BlockCount);
            Assert.Equal(8, descriptor.Size);
            Assert.Equal(1u, descriptor.StartBlock);
        }

        /// <summary>
        /// <see cref="ExtentDescriptor.WriteTo(byte[], int)"/> always throws.
        /// </summary>
        [Fact]
        public void WriteTo_Works()
        {
            var descriptor = new ExtentDescriptor()
            {
                StartBlock = 2,
                BlockCount = 1,
            };

            byte[] bytes = new byte[8];
            descriptor.WriteTo(bytes, 0);

            Assert.Equal(2, BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(0, 4)));
            Assert.Equal(1, BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(4, 4)));
        }
    }
}
