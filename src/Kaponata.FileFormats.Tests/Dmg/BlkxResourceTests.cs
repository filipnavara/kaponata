// <copyright file="BlkxResourceTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.Dmg;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Kaponata.FileFormats.Tests.Dmg
{
    /// <summary>
    /// Tests the <see cref="BlkxResource"/> class.
    /// </summary>
    public class BlkxResourceTests
    {
        /// <summary>
        /// The <see cref=" BlkxResource"/> properties work correctly.
        /// </summary>
        [Fact]
        public void Properties_Work()
        {
            var dict = new Dictionary<string, object>()
            {
                { "Attributes", "0x0050" },
                { "CFName", "Driver Descriptor Map (DDM : 0)" },
                { "Data", Convert.FromBase64String("bWlzaAAAAAEAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAIB/////wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAIAAAAg6P2c0P9/AAAwGb9f/38AAAAAAAAAAAAAwBi/X/9/AAAoAAAAAAAAAAAAAAAAAAAAAKAFAAEAAAAYAAAAAAAAACAkv1//fwAA8Bi/X/9/AACKWZeI/38AACAZv1//fwAAKAAAAAAAAAAAAAAAAAAAAFhAAgEBAAAAABm/X/9/AAAAAAACgAAABgAAAAAAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAOP////8AAAAAAAAAAAAAAAEAAAAAAAAAAAAAAAAAAAA4AAAAAAAAAAA=") },
                { "ID", "-1" },
                { "Name", "Driver Descriptor Map (DDM : 0)" },
            };

            var resource = new BlkxResource(dict);
            Assert.Equal(0x0050u, resource.Attributes);
            Assert.NotNull(resource.Block);
            Assert.Equal(-1, resource.Id);
            Assert.Equal("Driver Descriptor Map (DDM : 0)", resource.Name);
            Assert.Equal("blkx", resource.Type);
        }

        /// <summary>
        /// The <see cref=" BlkxResource"/> constructor throws when an invalid ID is passed.
        /// </summary>
        [Fact]
        public void Constructor_InvalidID_Throws()
        {
            var dict = new Dictionary<string, object>()
            {
                { "Attributes", "0x0050" },
                { "CFName", "Driver Descriptor Map (DDM : 0)" },
                { "Data", Convert.FromBase64String("bWlzaAAAAAEAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAIB/////wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAIAAAAg6P2c0P9/AAAwGb9f/38AAAAAAAAAAAAAwBi/X/9/AAAoAAAAAAAAAAAAAAAAAAAAAKAFAAEAAAAYAAAAAAAAACAkv1//fwAA8Bi/X/9/AACKWZeI/38AACAZv1//fwAAKAAAAAAAAAAAAAAAAAAAAFhAAgEBAAAAABm/X/9/AAAAAAACgAAABgAAAAAAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAOP////8AAAAAAAAAAAAAAAEAAAAAAAAAAAAAAAAAAAA4AAAAAAAAAAA=") },
                { "ID", "test" },
                { "Name", "Driver Descriptor Map (DDM : 0)" },
            };

            Assert.Throws<InvalidDataException>(() => new BlkxResource(dict));
        }

        /// <summary>
        /// The <see cref=" BlkxResource"/> constructor throws when invalid attributes are passed.
        /// </summary>
        [Fact]
        public void Constructor_InvalidAttributes_Throws()
        {
            var dict = new Dictionary<string, object>()
            {
                { "Attributes", "yada" },
                { "CFName", "Driver Descriptor Map (DDM : 0)" },
                { "Data", Convert.FromBase64String("bWlzaAAAAAEAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAIB/////wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAIAAAAg6P2c0P9/AAAwGb9f/38AAAAAAAAAAAAAwBi/X/9/AAAoAAAAAAAAAAAAAAAAAAAAAKAFAAEAAAAYAAAAAAAAACAkv1//fwAA8Bi/X/9/AACKWZeI/38AACAZv1//fwAAKAAAAAAAAAAAAAAAAAAAAFhAAgEBAAAAABm/X/9/AAAAAAACgAAABgAAAAAAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAOP////8AAAAAAAAAAAAAAAEAAAAAAAAAAAAAAAAAAAA4AAAAAAAAAAA=") },
                { "ID", "-1" },
                { "Name", "Driver Descriptor Map (DDM : 0)" },
            };

            Assert.Throws<InvalidDataException>(() => new BlkxResource(dict));
        }
    }
}
