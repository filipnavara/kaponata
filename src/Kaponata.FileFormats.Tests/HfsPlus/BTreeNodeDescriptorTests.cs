// <copyright file="BTreeNodeDescriptorTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.HfsPlus;
using System;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// Tests the <see cref="BTreeNodeDescriptor"/> class.
    /// </summary>
    public class BTreeNodeDescriptorTests
    {
        /// <summary>
        /// <see cref="BTreeNodeDescriptor.ReadFrom(byte[], int)"/> works correctly.
        /// </summary>
        [Fact]
        public void ReadFrom_Works()
        {
            BTreeNodeDescriptor descriptor = new BTreeNodeDescriptor();
            Assert.Equal(14, descriptor.ReadFrom(Convert.FromBase64String("AAAAAgAAAAD/AQAVAAA="), 0));

            Assert.Equal(0u, descriptor.BackwardLink);
            Assert.Equal(2u, descriptor.ForwardLink);
            Assert.Equal(1, descriptor.Height);
            Assert.Equal(BTreeNodeKind.LeafNode, descriptor.Kind);
            Assert.Equal(0x15, descriptor.NumRecords);
            Assert.Equal(0, descriptor.Reserved);
            Assert.Equal(14, descriptor.Size);
        }

        /// <summary>
        /// <see cref="BTreeNodeDescriptor.WriteTo(byte[], int)"/> works correctly.
        /// </summary>
        [Fact]
        public void WriteTo_Throws()
        {
            BTreeNodeDescriptor descriptor = new BTreeNodeDescriptor()
            {
                BackwardLink = 0,
                ForwardLink = 2,
                Height = 1,
                Kind = BTreeNodeKind.LeafNode,
                NumRecords = 0x15,
                Reserved = 0,
            };

            byte[] data = new byte[descriptor.Size];
            descriptor.WriteTo(data, 0);

            Assert.Equal("AAAAAgAAAAD/AQAVAAA=", Convert.ToBase64String(data));
        }
    }
}