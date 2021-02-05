// <copyright file="ForwardProtocol.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// Represents a protocol which is being forwarded over adb.
    /// </summary>
    public enum ForwardProtocol
    {
        /// <summary>
        /// Enables the forwarding of a TCP port.
        /// </summary>
        Tcp,

        /// <summary>
        /// Enables the forwarding of a Unix domain socket.
        /// </summary>
        LocalAbstract,

        /// <summary>
        /// Enables the forwarding of a Unix domain socket.
        /// </summary>
        LocalReserved,

        /// <summary>
        /// Enables the forwarding of a Unix domain socket.
        /// </summary>
        LocalFilesystem,

        /// <summary>
        /// Enables the forwarding of a character device.
        /// </summary>
        Device,

        /// <summary>
        /// Enables port forwarding of the java debugger for a specific process.
        /// </summary>
        JavaDebugWireProtocol,
    }
}
