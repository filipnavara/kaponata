// <copyright file="ForkDataTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.HfsPlus;
using System;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// Tests the <see cref="ForkData"/> class.
    /// </summary>
    public class ForkDataTests
    {
        /// <summary>
        /// <see cref="ForkData.ReadFrom(byte[], int)"/> works correctly.
        /// </summary>
        [Fact]
        public void ReadFrom_Works()
        {
            var fork = new ForkData();
            fork.ReadFrom(Convert.FromBase64String("AAAAAAAAEAAAABAAAAAAAQAAAAEAAAABAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA="), 0);

            Assert.Collection(
                fork.Extents,
                e =>
                {
                    Assert.Equal(1u, e.StartBlock);
                    Assert.Equal(1u, e.BlockCount);
                },
                e =>
                {
                    Assert.Equal(0u, e.StartBlock);
                    Assert.Equal(0u, e.BlockCount);
                },
                e =>
                {
                    Assert.Equal(0u, e.StartBlock);
                    Assert.Equal(0u, e.BlockCount);
                },
                e =>
                {
                    Assert.Equal(0u, e.StartBlock);
                    Assert.Equal(0u, e.BlockCount);
                },
                e =>
                {
                    Assert.Equal(0u, e.StartBlock);
                    Assert.Equal(0u, e.BlockCount);
                },
                e =>
                {
                    Assert.Equal(0u, e.StartBlock);
                    Assert.Equal(0u, e.BlockCount);
                },
                e =>
                {
                    Assert.Equal(0u, e.StartBlock);
                    Assert.Equal(0u, e.BlockCount);
                },
                e =>
                {
                    Assert.Equal(0u, e.StartBlock);
                    Assert.Equal(0u, e.BlockCount);
                });

            Assert.Equal(0x1000u, fork.LogicalSize);
            Assert.Equal(0x50, fork.Size);
            Assert.Equal(0x1u, fork.TotalBlocks);
        }

        /// <summary>
        /// <see cref="ForkData.WriteTo(byte[], int)"/> always throws.
        /// </summary>
        [Fact]
        public void WriteTo_Throws()
        {
            var fork = new ForkData();
            Assert.Throws<NotImplementedException>(() => fork.WriteTo(Array.Empty<byte>(), 0));
        }
    }
}
