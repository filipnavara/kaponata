//-----------------------------------------------------------------------
// <copyright file="XarFileTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Kaponata.FileFormats.Xar;
using System;
using System.IO;
using Xunit;

namespace Kaponata.FileFormats.Tests.Xar
{
    /// <summary>
    /// Tests the <see cref="XarFile"/> class.
    /// </summary>
    public class XarFileTests
    {
        /// <summary>
        /// The <see cref="XarFile"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new XarFile(null, true));
        }

        /// <summary>
        /// The <see cref="XarFile"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ThrowsOnEmptyStream()
        {
            Assert.Throws<InvalidDataException>(() => new XarFile(Stream.Null, true));
        }

        /// <summary>
        /// Tests the <see cref="XarFile.Open(string)"/> method by extracting a simple
        /// text file.
        /// </summary>
        [Fact]
        public void ExtractTestFile()
        {
            using (Stream stream = File.OpenRead("TestAssets/test.xar"))
            using (XarFile xar = new XarFile(stream, leaveOpen: true))
            using (Stream entryStream = xar.Open("dmg/hello.txt"))
            using (StreamReader reader = new StreamReader(entryStream))
            {
                var text = reader.ReadToEnd();

                Assert.Equal("Hello, World!\n", text);
            }
        }

        /// <summary>
        /// <see cref="XarFile.Open(string)"/> validates its arguments.
        /// </summary>
        [Fact]
        public void Open_ValidatesArgument()
        {
            using (Stream stream = File.OpenRead("TestAssets/test.xar"))
            using (XarFile xar = new XarFile(stream, leaveOpen: true))
            {
                Assert.Throws<ArgumentNullException>(() => xar.Open(null));
            }
        }

        /// <summary>
        /// <see cref="XarFile.Open(string)"/> throws when an invalid name is specified.
        /// </summary>
        /// <param name="name">
        /// The name of the entry to open.
        /// </param>
        [InlineData("")]
        [InlineData("dmg")]
        [InlineData("missing")]
        [InlineData("dmg/missing")]
        [InlineData("dmg/hello.txt/invalid")]
        [Theory]
        public void Open_MissingEntry_Throws(string name)
        {
            using (Stream stream = File.OpenRead("TestAssets/test.xar"))
            using (XarFile xar = new XarFile(stream, leaveOpen: true))
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => xar.Open(name));
            }
        }

        /// <summary>
        /// Tests the <see cref="XarFile.EntryNames"/> property.
        /// </summary>
        [Fact]
        public void GetFileNamesTest()
        {
            using (Stream stream = File.OpenRead("TestAssets/test.xar"))
            using (XarFile xar = new XarFile(stream, leaveOpen: true))
            {
                Assert.NotNull(xar.EntryNames);

                Assert.Collection(
                    xar.EntryNames,
                    e => Assert.Equal("dmg", e),
                    e => Assert.Equal("dmg/hello.txt", e));
            }
        }

        /// <summary>
        /// Tests the <see cref="XarFile.Files"/> property.
        /// </summary>
        [Fact]
        public void GetFilesTest()
        {
            using (Stream stream = File.OpenRead("TestAssets/test.xar"))
            using (XarFile xar = new XarFile(stream, leaveOpen: true))
            {
                var dmg = Assert.Single(xar.Files);

                Assert.Equal(1, dmg.Id);
                Assert.Equal(DateTimeOffset.Parse("2021-04-26T15:13:19Z"), dmg.Created);
                Assert.Equal(DateTimeOffset.Parse("2021-04-26T15:13:19Z"), dmg.Modified);
                Assert.Equal(DateTimeOffset.Parse("2021-04-26T15:13:39Z"), dmg.Archived);
                Assert.Equal("staff", dmg.Group);
                Assert.Equal(20, dmg.GroupId);
                Assert.Equal("runner", dmg.User);
                Assert.Equal(501, dmg.UserId);
                Assert.Equal(LinuxFileMode.S_IXOTH | LinuxFileMode.S_IROTH | LinuxFileMode.S_IXGRP | LinuxFileMode.S_IRGRP | LinuxFileMode.S_IXUSR | LinuxFileMode.S_IWUSR | LinuxFileMode.S_IRUSR, dmg.FileMode);
                Assert.Equal(16777220, dmg.DeviceNo);
                Assert.Equal(12896267367, dmg.Inode);
                Assert.Equal(XarEntryType.Directory, dmg.Type);
                Assert.Equal("dmg", dmg.Name);
                Assert.Equal("dmg", dmg.ToString());

                var hello = Assert.Single(dmg.Files);

                Assert.Equal(22, hello.DataLength);
                Assert.Equal(20, hello.DataOffset);
                Assert.Equal(14, hello.DataSize);
                Assert.Equal("application/x-gzip", hello.Encoding);
                Assert.Equal("sha1", hello.ExtractedChecksumStyle);
                Assert.Equal("60fde9c2310b0d4cad4dab8d126b04387efba289", hello.ExtractedChecksum);
                Assert.Equal("sha1", hello.ArchivedChecksumStyle);
                Assert.Equal("cc4beb49abd8a0651e8de85849b75508af1cce77", hello.ArchivedChecksum);
                Assert.Equal(DateTimeOffset.Parse("2021-04-26T15:13:19Z"), hello.Created);
                Assert.Equal(DateTimeOffset.Parse("2021-04-26T15:13:19Z"), hello.Modified);
                Assert.Equal(DateTimeOffset.Parse("2021-04-26T15:13:44Z"), hello.Archived);
                Assert.Equal("staff", hello.Group);
                Assert.Equal(20, hello.GroupId);
                Assert.Equal("runner", hello.User);
                Assert.Equal(501, hello.UserId);
                Assert.Equal(LinuxFileMode.S_IROTH | LinuxFileMode.S_IRGRP | LinuxFileMode.S_IWUSR | LinuxFileMode.S_IRUSR, hello.FileMode);
                Assert.Equal(16777220, hello.DeviceNo);
                Assert.Equal(12896267368, hello.Inode);
                Assert.Equal(XarEntryType.File, hello.Type);
                Assert.Equal("hello.txt", hello.Name);
                Assert.Equal("hello.txt", hello.ToString());
            }
        }

        /// <summary>
        /// <see cref="XarFile.Dispose"/> disposes on the inner stream.
        /// </summary>
        [Fact]
        public void Dispose_DisposesInner()
        {
            using (Stream stream = File.OpenRead("TestAssets/test.xar"))
            {
                XarFile file = new XarFile(stream, leaveOpen: false);
                file.Dispose();

                Assert.Throws<ObjectDisposedException>(() => stream.Position);
            }
        }
    }
}
