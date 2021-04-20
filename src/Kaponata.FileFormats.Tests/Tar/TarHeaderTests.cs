// <copyright file="TarHeaderTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.FileFormats.Tar;
using System;
using System.IO;
using Xunit;

namespace Kaponata.FileFormats.Tests.Tar
{
    /// <summary>
    /// Tests the <see cref="TarHeader"/> class.
    /// </summary>
    public class TarHeaderTests
    {
        // More test files can be found at https://github.com/golang/go/tree/master/src/archive/tar/testdata

        /// <summary>
        /// <see cref="TarHeader.Read(Span{byte})"/> can correctly parse a directory entry.
        /// </summary>
        [Fact]
        public void ReadHeader_Directory_Works()
        {
            var bytes = File.ReadAllBytes("Tar/test.tar");
            var header = TarHeader.Read(bytes.AsSpan(0, 0x200));

            Assert.Equal("./", header.FileName);
            Assert.Equal(LinuxFileMode.S_IXOTH | LinuxFileMode.S_IROTH | LinuxFileMode.S_IXGRP | LinuxFileMode.S_IRGRP | LinuxFileMode.S_IXUSR | LinuxFileMode.S_IWUSR | LinuxFileMode.S_IRUSR, header.FileMode);
            Assert.Equal(0u, header.UserId);
            Assert.Equal(0u, header.GroupId);
            Assert.Equal(0u, header.FileSize);
            Assert.Equal(new DateTimeOffset(2017, 10, 05, 17, 55, 58, TimeSpan.Zero), header.LastModified);
            Assert.Equal(4049u, header.Checksum);
            Assert.Equal(TarTypeFlag.DirType, header.TypeFlag);
            Assert.Equal(string.Empty, header.LinkName);
            Assert.Equal("ustar", header.Magic);
            Assert.Null(header.Version);
            Assert.Equal("root", header.UserName);
            Assert.Equal("root", header.GroupName);
            Assert.Null(header.DevMajor);
            Assert.Null(header.DevMinor);
            Assert.Equal(string.Empty, header.Prefix);
        }

        /// <summary>
        /// <see cref="TarHeader.Read(Span{byte})"/> can correctly parse a regular file entry.
        /// </summary>
        [Fact]
        public void ReadHeader_File_Works()
        {
            var bytes = File.ReadAllBytes("Tar/test.tar");
            var header = TarHeader.Read(bytes.AsSpan(0x400, 0x200));

            Assert.Equal("./testdir/test321", header.FileName);
            Assert.Equal(LinuxFileMode.S_IROTH | LinuxFileMode.S_IRGRP | LinuxFileMode.S_IWUSR | LinuxFileMode.S_IRUSR, header.FileMode);
            Assert.Equal(0u, header.UserId);
            Assert.Equal(0u, header.GroupId);
            Assert.Equal(8u, header.FileSize);
            Assert.Equal(new DateTimeOffset(2017, 10, 05, 17, 56, 14, TimeSpan.Zero), header.LastModified);
            Assert.Equal(5456u, header.Checksum);
            Assert.Equal(TarTypeFlag.RegType, header.TypeFlag);
            Assert.Equal(string.Empty, header.LinkName);
            Assert.Equal("ustar", header.Magic);
            Assert.Null(header.Version);
            Assert.Equal("root", header.UserName);
            Assert.Equal("root", header.GroupName);
            Assert.Null(header.DevMajor);
            Assert.Null(header.DevMinor);
            Assert.Equal(string.Empty, header.Prefix);
        }

        /// <summary>
        /// <see cref="TarHeader.Read(Span{byte})"/> can correctly parse a regular file entry.
        /// </summary>
        [Fact]
        public void ReadHeader_File2_Works()
        {
            var bytes = File.ReadAllBytes("Tar/rootfs.tar");
            var header = TarHeader.Read(bytes.AsSpan(0, 0x200));

            Assert.Equal("hello.txt", header.FileName);
            Assert.Equal(LinuxFileMode.S_IXOTH | LinuxFileMode.S_IROTH | LinuxFileMode.S_IXGRP | LinuxFileMode.S_IRGRP | LinuxFileMode.S_IXUSR | LinuxFileMode.S_IWUSR | LinuxFileMode.S_IRUSR | LinuxFileMode.S_IFREG, header.FileMode);
            Assert.Equal(0u, header.UserId);
            Assert.Equal(0u, header.GroupId);
            Assert.Equal(15u, header.FileSize);
            Assert.Equal(new DateTimeOffset(2021, 04, 19, 15, 13, 11, TimeSpan.Zero), header.LastModified);
            Assert.Equal(4684u, header.Checksum);
            Assert.Equal(TarTypeFlag.RegType, header.TypeFlag);
            Assert.Equal(string.Empty, header.LinkName);
            Assert.Equal("ustar", header.Magic);
            Assert.Equal(0u, header.Version);
            Assert.Equal(string.Empty, header.UserName);
            Assert.Equal(string.Empty, header.GroupName);
            Assert.Equal(0u, header.DevMajor);
            Assert.Equal(0u, header.DevMinor);
            Assert.Equal(string.Empty, header.Prefix);
        }

        /// <summary>
        /// <see cref="TarHeader.Read(Span{byte})"/> throws when presented with insufficient bytes.
        /// </summary>
        [Fact]
        public void ReadHeader_InvalidData_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TarHeader.Read(Array.Empty<byte>()));
        }

        /// <summary>
        /// <see cref="TarHeader.Read(Span{byte})"/> can correctly parse a directory entry.
        /// </summary>
        [Fact]
        public void WriteHeader_Directory_Works()
        {
            var bytes = File.ReadAllBytes("Tar/test.tar");
            TarHeader header = default;

            header.FileName = "./";
            header.FileMode = LinuxFileMode.S_IXOTH | LinuxFileMode.S_IROTH | LinuxFileMode.S_IXGRP | LinuxFileMode.S_IRGRP | LinuxFileMode.S_IXUSR | LinuxFileMode.S_IWUSR | LinuxFileMode.S_IRUSR;
            header.UserId = 0;
            header.GroupId = 0;
            header.FileSize = 0;
            header.LastModified = new DateTimeOffset(2017, 10, 05, 17, 55, 58, TimeSpan.Zero);
            header.TypeFlag = TarTypeFlag.DirType;
            header.LinkName = string.Empty;
            header.Magic = "ustar";
            header.Version = null;
            header.UserName = "root";
            header.GroupName = "root";
            header.DevMajor = null;
            header.DevMinor = null;
            header.Prefix = string.Empty;

            byte[] buffer = new byte[512];
            header.Write(buffer);

            Assert.Equal(bytes.AsSpan(0, 512).ToArray(), buffer);
        }

        /// <summary>
        /// <see cref="TarHeader.Read(Span{byte})"/> can correctly parse a regular file entry.
        /// </summary>
        [Fact]
        public void WriteHeader_File_Works()
        {
            var bytes = File.ReadAllBytes("Tar/test.tar");
            TarHeader header = default;

            header.FileName = "./testdir/test321";
            header.FileMode = LinuxFileMode.S_IROTH | LinuxFileMode.S_IRGRP | LinuxFileMode.S_IWUSR | LinuxFileMode.S_IRUSR;
            header.UserId = 0;
            header.GroupId = 0;
            header.FileSize = 8;
            header.LastModified = new DateTimeOffset(2017, 10, 05, 17, 56, 14, TimeSpan.Zero);
            header.TypeFlag = TarTypeFlag.RegType;
            header.LinkName = string.Empty;
            header.Magic = "ustar";
            header.Version = null;
            header.UserName = "root";
            header.GroupName = "root";
            header.DevMajor = null;
            header.DevMinor = null;
            header.Prefix = string.Empty;

            byte[] buffer = new byte[512];
            header.Write(buffer);

            Assert.Equal(bytes.AsSpan(0x400, 0x200).ToArray(), buffer);
        }

        /// <summary>
        /// <see cref="TarHeader.Read(Span{byte})"/> can correctly parse a regular file entry.
        /// </summary>
        [Fact]
        public void WriteHeader_File2_Works()
        {
            var bytes = File.ReadAllBytes("Tar/rootfs.tar");
            TarHeader header = default;

            header.FileName = "hello.txt";
            header.FileMode = LinuxFileMode.S_IXOTH | LinuxFileMode.S_IROTH | LinuxFileMode.S_IXGRP | LinuxFileMode.S_IRGRP | LinuxFileMode.S_IXUSR | LinuxFileMode.S_IWUSR | LinuxFileMode.S_IRUSR | LinuxFileMode.S_IFREG;
            header.UserId = 0;
            header.GroupId = 0;
            header.FileSize = 15;
            header.LastModified = new DateTimeOffset(2021, 04, 19, 15, 13, 11, TimeSpan.Zero);
            header.TypeFlag = TarTypeFlag.RegType;
            header.LinkName = string.Empty;
            header.Magic = "ustar\0";
            header.Version = 0;
            header.UserName = string.Empty;
            header.GroupName = string.Empty;
            header.DevMajor = 0;
            header.DevMinor = 0;
            header.Prefix = string.Empty;

            byte[] buffer = new byte[512];
            header.Write(buffer);

            Assert.Equal(bytes.AsSpan(0, 0x200).ToArray(), buffer);
        }

        /// <summary>
        /// <see cref="TarHeader.Read(Span{byte})"/> throws when presented with insufficient bytes.
        /// </summary>
        [Fact]
        public void WriteHeader_InvalidData_Throws()
        {
            TarHeader header = default;
            Assert.Throws<ArgumentOutOfRangeException>(() => header.Write(Array.Empty<byte>()));
        }
    }
}
