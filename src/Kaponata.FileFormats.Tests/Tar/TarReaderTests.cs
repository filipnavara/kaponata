// <copyright file="TarReaderTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.FileFormats.Tar;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.FileFormats.Tests.Tar
{
    /// <summary>
    /// Tests the <see cref="TarReader"/> class.
    /// </summary>
    public class TarReaderTests
    {
        // More test files can be found at https://github.com/golang/go/tree/master/src/archive/tar/testdata

        /// <summary>
        /// The <see cref="TarReader"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new TarReader(null));
        }

        /// <summary>
        /// The <c>rootfs.tar</c> archive can be processed correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Read_Rootfs_Works_Async()
        {
            using (Stream stream = File.OpenRead("Tar/rootfs.tar"))
            {
                TarReader reader = new TarReader(stream);

                TarHeader? entryHeader;
                Stream entryStream;

                Collection<Stream> streams = new Collection<Stream>();
                Collection<TarHeader> headers = new Collection<TarHeader>();

                while (((entryHeader, entryStream) = await reader.ReadAsync(default)).entryHeader != null)
                {
                    streams.Add(entryStream);
                    headers.Add(entryHeader.Value);
                }

                Assert.Collection(
                    streams,
                    s =>
                    {
                        // The stream should have been disposed of.
                        Assert.False(s.CanRead);
                    },
                    s =>
                    {
                        // The stream should have been disposed of.
                        Assert.False(s.CanRead);
                    },
                    s =>
                    {
                        // The stream should have been disposed of.
                        Assert.False(s.CanRead);
                    });

                Assert.Collection(
                    headers,
                    h =>
                    {
                        Assert.Equal("hello.txt", h.FileName);
                        Assert.Equal(LinuxFileMode.S_IXOTH | LinuxFileMode.S_IROTH | LinuxFileMode.S_IXGRP | LinuxFileMode.S_IRGRP | LinuxFileMode.S_IXUSR | LinuxFileMode.S_IWUSR | LinuxFileMode.S_IRUSR | LinuxFileMode.S_IFREG, h.FileMode);
                        Assert.Equal(0u, h.UserId);
                        Assert.Equal(0u, h.GroupId);
                        Assert.Equal(15u, h.FileSize);
                        Assert.Equal(new DateTimeOffset(2021, 4, 19, 15, 13, 11, TimeSpan.Zero), h.LastModified);
                        Assert.Equal(4684u, h.Checksum);
                        Assert.Equal(TarTypeFlag.RegType, h.TypeFlag);
                        Assert.Equal(string.Empty, h.LinkName);
                        Assert.Equal("ustar", h.Magic);
                        Assert.Equal(string.Empty, h.UserName);
                        Assert.Equal(string.Empty, h.GroupName);
                        Assert.Equal(0u, h.DevMajor);
                        Assert.Equal(0u, h.DevMinor);
                        Assert.Equal(string.Empty, h.Prefix);
                    },
                    h =>
                    {
                        Assert.Equal(string.Empty, h.FileName);
                        Assert.Equal(LinuxFileMode.None, h.FileMode);
                        Assert.Equal(0u, h.UserId);
                        Assert.Equal(0u, h.GroupId);
                        Assert.Equal(0u, h.FileSize);
                        Assert.Equal(DateTimeOffset.UnixEpoch, h.LastModified);
                        Assert.Equal(0u, h.Checksum);
                        Assert.Equal(TarTypeFlag.ARegType, h.TypeFlag);
                        Assert.Equal(string.Empty, h.LinkName);
                        Assert.Equal(string.Empty, h.Magic);
                        Assert.Equal(string.Empty, h.UserName);
                        Assert.Equal(string.Empty, h.GroupName);
                        Assert.Null(h.DevMajor);
                        Assert.Null(h.DevMinor);
                        Assert.Equal(string.Empty, h.Prefix);
                    },
                    h =>
                    {
                        Assert.Equal(string.Empty, h.FileName);
                        Assert.Equal(LinuxFileMode.None, h.FileMode);
                        Assert.Equal(0u, h.UserId);
                        Assert.Equal(0u, h.GroupId);
                        Assert.Equal(0u, h.FileSize);
                        Assert.Equal(DateTimeOffset.UnixEpoch, h.LastModified);
                        Assert.Equal(0u, h.Checksum);
                        Assert.Equal(TarTypeFlag.ARegType, h.TypeFlag);
                        Assert.Equal(string.Empty, h.LinkName);
                        Assert.Equal(string.Empty, h.Magic);
                        Assert.Equal(string.Empty, h.UserName);
                        Assert.Equal(string.Empty, h.GroupName);
                        Assert.Null(h.DevMajor);
                        Assert.Null(h.DevMinor);
                        Assert.Equal(string.Empty, h.Prefix);
                    });
            }
        }

        /// <summary>
        /// The <c>test.tar</c> archive can be processed correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Read_Test_Works_Async()
        {
            using (Stream stream = File.OpenRead("Tar/test.tar"))
            {
                TarReader reader = new TarReader(stream);

                TarHeader? entryHeader;
                Stream entryStream;

                Collection<Stream> streams = new Collection<Stream>();
                Collection<TarHeader> headers = new Collection<TarHeader>();

                while (((entryHeader, entryStream) = await reader.ReadAsync(default)).entryHeader != null)
                {
                    streams.Add(entryStream);
                    headers.Add(entryHeader.Value);
                }

                Assert.Collection(
                    streams,
                    s =>
                    {
                        // The stream should have been disposed of.
                        Assert.False(s.CanRead);
                    },
                    s =>
                    {
                        // The stream should have been disposed of.
                        Assert.False(s.CanRead);
                    },
                    s =>
                    {
                        // The stream should have been disposed of.
                        Assert.False(s.CanRead);
                    },
                    s =>
                    {
                        // The stream should have been disposed of.
                        Assert.False(s.CanRead);
                    },
                    s =>
                    {
                        // The stream should have been disposed of.
                        Assert.False(s.CanRead);
                    },
                    s =>
                    {
                        // The stream should have been disposed of.
                        Assert.False(s.CanRead);
                    });

                Assert.Collection(
                    headers,
                    h =>
                    {
                        Assert.Equal("./", h.FileName);
                        Assert.Equal(LinuxFileMode.S_IXOTH | LinuxFileMode.S_IROTH | LinuxFileMode.S_IXGRP | LinuxFileMode.S_IRGRP | LinuxFileMode.S_IXUSR | LinuxFileMode.S_IWUSR | LinuxFileMode.S_IRUSR, h.FileMode);
                        Assert.Equal(0u, h.UserId);
                        Assert.Equal(0u, h.GroupId);
                        Assert.Equal(0u, h.FileSize);
                        Assert.Equal(new DateTimeOffset(2017, 10, 5, 17, 55, 58, TimeSpan.Zero), h.LastModified);
                        Assert.Equal(4049u, h.Checksum);
                        Assert.Equal(TarTypeFlag.DirType, h.TypeFlag);
                        Assert.Equal(string.Empty, h.LinkName);
                        Assert.Equal("ustar", h.Magic);
                        Assert.Equal("root", h.UserName);
                        Assert.Equal("root", h.GroupName);
                        Assert.Null(h.DevMajor);
                        Assert.Null(h.DevMinor);
                        Assert.Equal(string.Empty, h.Prefix);
                    },
                    h =>
                    {
                        Assert.Equal("./testdir/", h.FileName);
                        Assert.Equal(LinuxFileMode.S_IXOTH | LinuxFileMode.S_IROTH | LinuxFileMode.S_IXGRP | LinuxFileMode.S_IRGRP | LinuxFileMode.S_IXUSR | LinuxFileMode.S_IWUSR | LinuxFileMode.S_IRUSR, h.FileMode);
                        Assert.Equal(0u, h.UserId);
                        Assert.Equal(0u, h.GroupId);
                        Assert.Equal(0u, h.FileSize);
                        Assert.Equal(new DateTimeOffset(2017, 10, 5, 17, 56, 14, TimeSpan.Zero), h.LastModified);
                        Assert.Equal(4865u, h.Checksum);
                        Assert.Equal(TarTypeFlag.DirType, h.TypeFlag);
                        Assert.Equal(string.Empty, h.LinkName);
                        Assert.Equal("ustar", h.Magic);
                        Assert.Equal("root", h.UserName);
                        Assert.Equal("root", h.GroupName);
                        Assert.Null(h.DevMajor);
                        Assert.Null(h.DevMinor);
                        Assert.Equal(string.Empty, h.Prefix);
                    },
                    h =>
                    {
                        Assert.Equal("./testdir/test321", h.FileName);
                        Assert.Equal(LinuxFileMode.S_IROTH | LinuxFileMode.S_IRGRP | LinuxFileMode.S_IWUSR | LinuxFileMode.S_IRUSR, h.FileMode);
                        Assert.Equal(0u, h.UserId);
                        Assert.Equal(0u, h.GroupId);
                        Assert.Equal(8u, h.FileSize);
                        Assert.Equal(new DateTimeOffset(2017, 10, 5, 17, 56, 14, TimeSpan.Zero), h.LastModified);
                        Assert.Equal(5456u, h.Checksum);
                        Assert.Equal(TarTypeFlag.RegType, h.TypeFlag);
                        Assert.Equal(string.Empty, h.LinkName);
                        Assert.Equal("ustar", h.Magic);
                        Assert.Equal("root", h.UserName);
                        Assert.Equal("root", h.GroupName);
                        Assert.Null(h.DevMajor);
                        Assert.Null(h.DevMinor);
                        Assert.Equal(string.Empty, h.Prefix);
                    },
                    h =>
                    {
                        Assert.Equal("./test", h.FileName);
                        Assert.Equal(LinuxFileMode.S_IROTH | LinuxFileMode.S_IRGRP | LinuxFileMode.S_IWUSR | LinuxFileMode.S_IRUSR, h.FileMode);
                        Assert.Equal(0u, h.UserId);
                        Assert.Equal(0u, h.GroupId);
                        Assert.Equal(8u, h.FileSize);
                        Assert.Equal(new DateTimeOffset(2017, 10, 5, 17, 55, 50, TimeSpan.Zero), h.LastModified);
                        Assert.Equal(4489u, h.Checksum);
                        Assert.Equal(TarTypeFlag.RegType, h.TypeFlag);
                        Assert.Equal(string.Empty, h.LinkName);
                        Assert.Equal("ustar", h.Magic);
                        Assert.Equal("root", h.UserName);
                        Assert.Equal("root", h.GroupName);
                        Assert.Null(h.DevMajor);
                        Assert.Null(h.DevMinor);
                        Assert.Equal(string.Empty, h.Prefix);
                    },
                    h =>
                    {
                        Assert.Equal(string.Empty, h.FileName);
                        Assert.Equal(LinuxFileMode.None, h.FileMode);
                        Assert.Equal(0u, h.UserId);
                        Assert.Equal(0u, h.GroupId);
                        Assert.Equal(0u, h.FileSize);
                        Assert.Equal(DateTimeOffset.UnixEpoch, h.LastModified);
                        Assert.Equal(0u, h.Checksum);
                        Assert.Equal(TarTypeFlag.ARegType, h.TypeFlag);
                        Assert.Equal(string.Empty, h.LinkName);
                        Assert.Equal(string.Empty, h.Magic);
                        Assert.Equal(string.Empty, h.UserName);
                        Assert.Equal(string.Empty, h.GroupName);
                        Assert.Null(h.DevMajor);
                        Assert.Null(h.DevMinor);
                        Assert.Equal(string.Empty, h.Prefix);
                    },
                    h =>
                    {
                        Assert.Equal(string.Empty, h.FileName);
                        Assert.Equal(LinuxFileMode.None, h.FileMode);
                        Assert.Equal(0u, h.UserId);
                        Assert.Equal(0u, h.GroupId);
                        Assert.Equal(0u, h.FileSize);
                        Assert.Equal(DateTimeOffset.UnixEpoch, h.LastModified);
                        Assert.Equal(0u, h.Checksum);
                        Assert.Equal(TarTypeFlag.ARegType, h.TypeFlag);
                        Assert.Equal(string.Empty, h.LinkName);
                        Assert.Equal(string.Empty, h.Magic);
                        Assert.Equal(string.Empty, h.UserName);
                        Assert.Equal(string.Empty, h.GroupName);
                        Assert.Null(h.DevMajor);
                        Assert.Null(h.DevMinor);
                        Assert.Equal(string.Empty, h.Prefix);
                    });
            }
        }
    }
}
