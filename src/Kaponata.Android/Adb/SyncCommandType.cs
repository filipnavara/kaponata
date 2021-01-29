// <copyright file="SyncCommandType.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// Defines a command that can be sent to, or a response that can be received from, the sync service.
    /// </summary>
    public enum SyncCommandType
    {
        /// <summary>
        /// List the files in a folder.
        /// </summary>
        LIST = 0x4c495354,

        /// <summary>
        /// Retrieve a file from device
        /// </summary>
        RECV = 0x52454356,

        /// <summary>
        /// Send a file to device
        /// </summary>
        SEND = 0x53454e44,

        /// <summary>
        /// Stat a file
        /// </summary>
        STAT = 0x53544154,

        /// <summary>
        /// A directory entry.
        /// </summary>
        DENT = 0x44454e54,

        /// <summary>
        /// The operation has failed.
        /// </summary>
        FAIL = 0x4641494c,

        /// <summary>
        /// Marks the start of a data packet.
        /// </summary>
        DATA = 0x44415441,

        /// <summary>
        /// The server has acknowledged the request.
        /// </summary>
        OKAY = 0x4f4b4159,

        /// <summary>
        /// The operation has completed.
        /// </summary>
        DONE = 0x444f4e45,
    }
}
