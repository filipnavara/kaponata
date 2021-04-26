// <copyright file="AttributeInlineDataTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.HfsPlus;
using System;
using System.IO;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// Tests the <see cref="AttributeInlineData"/> class.
    /// </summary>
    public class AttributeInlineDataTests
    {
        /// <summary>
        /// <see cref="AttributeInlineData.ReadFrom(byte[], int)"/> correctly interprets the data.
        /// </summary>
        [Fact]
        public void Read_Test()
        {
            byte[] buffer = Convert.FromBase64String("AAAAEAAAAAAAAAAAAAAB+w==");

            AttributeInlineData data = new AttributeInlineData();
            data.ReadFrom(buffer, 0);
            Assert.Equal(0x1fbu, data.LogicalSize);
            Assert.Equal(AttributeRecordType.InlineData, data.RecordType);
            Assert.Equal(0u, data.Reserved1);
            Assert.Equal(0u, data.Reserved2);
            Assert.Equal(16, data.Size);
        }

        /// <summary>
        /// <see cref="AttributeInlineData.ReadFrom(byte[], int)"/> throws when the data does not represent an inline data attribute.
        /// </summary>
        [Fact]
        public void Read_Invalid_Throws()
        {
            byte[] buffer = Convert.FromBase64String("BAAAEAAAAAAAAAAAAAAB+w==");

            AttributeInlineData data = new AttributeInlineData();
            Assert.Throws<InvalidDataException>(() => data.ReadFrom(buffer, 0));
        }

        /// <summary>
        /// <see cref="AttributeData.WriteTo(byte[], int)"/> throws.
        /// </summary>
        [Fact]
        public void Write_Throws()
        {
            var data = new AttributeInlineData();
            Assert.Throws<NotImplementedException>(() => data.WriteTo(Array.Empty<byte>(), 0));
        }
    }
}
