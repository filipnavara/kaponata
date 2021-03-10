// <copyright file="UnixMarchalTest.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Multimedia.FFmpeg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Kaponata.Multimedia.Tests
{
    /// <summary>
    /// Tests the <see cref="UnixMarshal"/> class.
    /// </summary>
    public class UnixMarchalTest
    {
        /// <summary>
        /// The <see cref="UnixMarshal.GetErrorMessage(UnixError)"/> method throws an exception for an error code for which no
        /// value is defined in the <see cref="UnixError"/> enumeration.
        /// </summary>
        [Fact]
        public void GetErrorMessage_ThrowsOnUnkownCode()
        {
            foreach (var error in Enum.GetValues<UnixError>())
            {
                Assert.False(string.IsNullOrEmpty(UnixMarshal.GetErrorMessage(error)));
            }

            Assert.Equal("An Unix error occurred. Error code: 1234", UnixMarshal.GetErrorMessage((UnixError)1234));
        }

        /// <summary>
        /// The <see cref="UnixMarshal.ThrowExceptionForError(UnixError)"/> method throws the right exception.
        /// </summary>
        [Fact]
        public void ThrowExceptionForError_Throws()
        {
            Assert.Throws<ArgumentException>(() => UnixMarshal.ThrowExceptionForError(UnixError.EBADF));
            Assert.Throws<ArgumentException>(() => UnixMarshal.ThrowExceptionForError(UnixError.EINVAL));
            Assert.Throws<ArgumentOutOfRangeException>(() => UnixMarshal.ThrowExceptionForError(UnixError.ERANGE));
            Assert.Throws<DirectoryNotFoundException>(() => UnixMarshal.ThrowExceptionForError(UnixError.ENOTDIR));
            Assert.Throws<FileNotFoundException>(() => UnixMarshal.ThrowExceptionForError(UnixError.ENOENT));
            Assert.Throws<InvalidOperationException>(() => UnixMarshal.ThrowExceptionForError(UnixError.EOPNOTSUPP));
            Assert.Throws<InvalidOperationException>(() => UnixMarshal.ThrowExceptionForError(UnixError.EOPNOTSUPP));
            Assert.Throws<InvalidOperationException>(() => UnixMarshal.ThrowExceptionForError(UnixError.EPERM));
            Assert.Throws<InvalidProgramException>(() => UnixMarshal.ThrowExceptionForError(UnixError.ENOEXEC));
            Assert.Throws<IOException>(() => UnixMarshal.ThrowExceptionForError(UnixError.EIO));
            Assert.Throws<IOException>(() => UnixMarshal.ThrowExceptionForError(UnixError.ENOSPC));
            Assert.Throws<IOException>(() => UnixMarshal.ThrowExceptionForError(UnixError.ENOTEMPTY));
            Assert.Throws<IOException>(() => UnixMarshal.ThrowExceptionForError(UnixError.ENXIO));
            Assert.Throws<IOException>(() => UnixMarshal.ThrowExceptionForError(UnixError.EROFS));
            Assert.Throws<IOException>(() => UnixMarshal.ThrowExceptionForError(UnixError.ESPIPE));
            Assert.Throws<NullReferenceException>(() => UnixMarshal.ThrowExceptionForError(UnixError.EFAULT));
            Assert.Throws<OverflowException>(() => UnixMarshal.ThrowExceptionForError(UnixError.EOVERFLOW));
            Assert.Throws<PathTooLongException>(() => UnixMarshal.ThrowExceptionForError(UnixError.ENAMETOOLONG));
            Assert.Throws<UnauthorizedAccessException>(() => UnixMarshal.ThrowExceptionForError(UnixError.EACCES));
            Assert.Throws<UnauthorizedAccessException>(() => UnixMarshal.ThrowExceptionForError(UnixError.EISDIR));
            Assert.Throws<Exception>(() => UnixMarshal.ThrowExceptionForError((UnixError)1234));
        }
    }
}
