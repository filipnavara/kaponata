// <copyright file="ForwardSpec.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// Represents an adb forward specification as used by the various adb port forwarding
    /// functions.
    /// </summary>
    public partial class ForwardSpec
    {
        /// <summary>
        /// Gets or sets the protocol which is being forwarded.
        /// </summary>
        public ForwardProtocol Protocol
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets, when the <see cref="Protocol"/> is <see cref="ForwardProtocol.Tcp"/>, the port
        /// number of the port being forwarded.
        /// </summary>
        public int Port
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets, when the <see cref="Protocol"/> is <see cref="ForwardProtocol.LocalAbstract"/>,
        /// <see cref="ForwardProtocol.LocalReserved"/> or <see cref="ForwardProtocol.LocalFilesystem"/>,
        /// the Unix domain socket name of the socket being forwarded.
        /// </summary>
        public string SocketName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets, when the <see cref="Protocol"/> is <see cref="ForwardProtocol.JavaDebugWireProtocol"/>,
        /// the process id of the process to which the debugger is attached.
        /// </summary>
        public int ProcessId
        {
            get;
            set;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine((int)this.Protocol, this.Port, this.ProcessId, this.SocketName);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var other = obj as ForwardSpec;

            if (other == null)
            {
                return false;
            }

            if (other.Protocol != this.Protocol)
            {
                return false;
            }

            switch (this.Protocol)
            {
                case ForwardProtocol.JavaDebugWireProtocol:
                    return this.ProcessId == other.ProcessId;
                case ForwardProtocol.Tcp:
                    return this.Port == other.Port;
                default:
                    return string.Equals(this.SocketName, other.SocketName);
            }
        }
    }
}
