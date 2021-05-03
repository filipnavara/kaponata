// <copyright file="AttributeKeyTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.HfsPlus;
using System;
using Xunit;

namespace Kaponata.FileFormats.Tests.HfsPlus
{
    /// <summary>
    /// Tests the <see cref="AttributeKey"/> class.
    /// </summary>
    public class AttributeKeyTests
    {
        /// <summary>
        /// <see cref="AttributeKey.ReadFrom(byte[], int)"/> works correctly.
        /// </summary>
        [Fact]
        public void ReadFrom_Works()
        {
            var data = Convert.FromBase64String("AC4AAAAAABcAAAAAABEAYwBvAG0ALgBhAHAAcABsAGUALgBkAGUAYwBtAHAAZgBz");

            var key = new AttributeKey();
            key.ReadFrom(data, 0);

            Assert.Equal(new CatalogNodeId(23), key.FileId);
            Assert.Equal("com.apple.decmpfs", key.Name);
            Assert.Equal(0x30, key.Size);

            Assert.Equal("com.apple.decmpfs (23)", key.ToString());
        }

        /// <summary>
        /// <see cref="AttributeKey.WriteTo(byte[], int)"/> always throws.
        /// </summary>
        [Fact]
        public void WriteTo_Throws()
        {
            var key = new AttributeKey();
            Assert.Throws<NotImplementedException>(() => key.WriteTo(Array.Empty<byte>(), 0));
        }

        /// <summary>
        /// <see cref="AttributeKey.CompareTo(BTreeKey)"/> throws when the other value is not an attribute key.
        /// </summary>
        [Fact]
        public void CompareTo_Invalid_Throws()
        {
            var key = new AttributeKey();

            Assert.Throws<ArgumentNullException>(() => key.CompareTo(null));
            Assert.Throws<ArgumentNullException>(() => key.CompareTo(new CatalogKey()));
        }

        /// <summary>
        /// <see cref="AttributeKey.CompareTo(BTreeKey)"/> returns the correct values.
        /// </summary>
        [Fact]
        public void CompareTo_Works()
        {
            var key = new AttributeKey();
            key.ReadFrom(Convert.FromBase64String("AC4AAAAAABcAAAAAABEAYwBvAG0ALgBhAHAAcABsAGUALgBkAGUAYwBtAHAAZgBz"), 0);

            var other = new AttributeKey();
            key.ReadFrom(Convert.FromBase64String("AC4AAAAAABcAAAAAABEAYwBvAG0ALgBhAHAAcABsAGUALgBkAGUAYwBtAHAAZgBz"), 0);

            Assert.Equal(0, key.CompareTo(key));
            Assert.Equal(1, key.CompareTo(other));
            Assert.Equal(-1, other.CompareTo(key));
        }

        /// <summary>
        /// <see cref="AttributeKey.Equals(object)"/> returns the correct values.
        /// </summary>
        [Fact]
        public void Equal_Works()
        {
            var key = new AttributeKey();
            key.ReadFrom(Convert.FromBase64String("AC4AAAAAABcAAAAAABEAYwBvAG0ALgBhAHAAcABsAGUALgBkAGUAYwBtAHAAZgBz"), 0);

            var other = new AttributeKey();
            key.ReadFrom(Convert.FromBase64String("AC4AAAAAABcAAAAAABEAYwBvAG0ALgBhAHAAcABsAGUALgBkAGUAYwBtAHAAZgBz"), 0);

            Assert.True(key.Equals(key));
            Assert.False(other.Equals(key));
            Assert.False(key.Equals(other));
        }

        /// <summary>
        /// <see cref="AttributeKey.GetHashCode"/> returns the correct values.
        /// </summary>
        [Fact]
        public void HashCode_Works()
        {
            var key = new AttributeKey();
            key.ReadFrom(Convert.FromBase64String("AC4AAAAAABcAAAAAABEAYwBvAG0ALgBhAHAAcABsAGUALgBkAGUAYwBtAHAAZgBz"), 0);

            var clone = new AttributeKey();
            clone.ReadFrom(Convert.FromBase64String("AC4AAAAAABcAAAAAABEAYwBvAG0ALgBhAHAAcABsAGUALgBkAGUAYwBtAHAAZgBz"), 0);

            var other = new AttributeKey();
            key.ReadFrom(Convert.FromBase64String("AC4AAAAAABcAAAAAABEAYwBvAG0ALgBhAHAAcABsAGUALgBkAGUAYwBtAHAAZgBz"), 0);

            Assert.Equal(key.GetHashCode(), key.GetHashCode());
            Assert.Equal(key.GetHashCode(), clone.GetHashCode());
            Assert.NotEqual(other.GetHashCode(), key.GetHashCode());
        }
    }
}
