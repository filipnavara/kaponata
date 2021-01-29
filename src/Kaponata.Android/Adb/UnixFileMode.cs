// <copyright file="UnixFileMode.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// Describes the properties of a file on an Android device.
    /// </summary>
    public enum UnixFileMode
    {
        /// <summary>
        /// The file is a Unix socket.
        /// </summary>
        Socket = 0x0e000,

        /// <summary>
        /// The file is a symbolic link.
        /// </summary>
        SymbolicLink = 0x0c000,

        /// <summary>
        /// The file is a regular file.
        /// </summary>
        Regular = 0x0a000,

        /// <summary>
        /// The file is a block device.
        /// </summary>
        Block = 0x6000,

        /// <summary>
        /// The file is a directory.
        /// </summary>
        Directory = 0x4000,

        /// <summary>
        /// The file is a character device.
        /// </summary>
        Character = 0x2000,

        /// <summary>
        /// The file is a first-in first-out queue.
        /// </summary>
        FIFO = 0x1000,
    }
}
