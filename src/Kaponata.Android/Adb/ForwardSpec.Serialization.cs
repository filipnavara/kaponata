// <copyright file="ForwardSpec.Serialization.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// Contains the serialization methods for the <see cref="ForwardSpec"/> class.
    /// </summary>
    public partial class ForwardSpec
    {
        /// <summary>
        /// Provides a mapping between a <see cref="string"/> and a <see cref="ForwardProtocol"/>
        /// value, used for the <see cref="string"/> representation of the <see cref="ForwardSpec"/>
        /// class.
        /// </summary>
        private static readonly Dictionary<string, ForwardProtocol> Mappings
            = new Dictionary<string, ForwardProtocol>(StringComparer.OrdinalIgnoreCase)
                {
                    { "tcp", ForwardProtocol.Tcp },
                    { "localabstract", ForwardProtocol.LocalAbstract },
                    { "localreserved", ForwardProtocol.LocalReserved },
                    { "localfilesystem", ForwardProtocol.LocalFilesystem },
                    { "dev", ForwardProtocol.Device },
                    { "jdwp", ForwardProtocol.JavaDebugWireProtocol },
                };

        /// <summary>
        /// Creates a <see cref="ForwardSpec"/> from its <see cref="string"/> representation.
        /// </summary>
        /// <param name="spec">
        /// A <see cref="string"/> which represents a <see cref="ForwardSpec"/>.
        /// </param>
        /// <returns>
        /// A <see cref="ForwardSpec"/> which represents <paramref name="spec"/>.
        /// </returns>
        public static ForwardSpec Parse(string spec)
        {
            if (spec == null)
            {
                throw new ArgumentNullException(nameof(spec));
            }

            var parts = spec.Split(new char[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
            {
                throw new ArgumentOutOfRangeException(nameof(spec));
            }

            if (!Mappings.ContainsKey(parts[0]))
            {
                throw new ArgumentOutOfRangeException(nameof(spec));
            }

            var protocol = Mappings[parts[0]];

            ForwardSpec value = new ForwardSpec()
            {
                Protocol = protocol,
            };

            int intValue;
            bool isInt = int.TryParse(parts[1], out intValue);

            switch (protocol, isInt)
            {
                case (ForwardProtocol.JavaDebugWireProtocol, true):
                    value.ProcessId = intValue;
                    break;
                case (ForwardProtocol.JavaDebugWireProtocol, false):
                    throw new ArgumentOutOfRangeException(nameof(spec));
                case (ForwardProtocol.Tcp, true):
                    value.Port = intValue;
                    break;
                case (ForwardProtocol.Tcp, false):
                    throw new ArgumentOutOfRangeException(nameof(spec));
                case (ForwardProtocol.LocalAbstract, _):
                case (ForwardProtocol.LocalFilesystem, _):
                case (ForwardProtocol.LocalReserved, _):
                case (ForwardProtocol.Device, _):
                    value.SocketName = parts[1];
                    break;
            }

            return value;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var protocolString = Mappings.FirstOrDefault(v => v.Value == this.Protocol).Key;

            switch (this.Protocol)
            {
                case ForwardProtocol.JavaDebugWireProtocol:
                    return $"{protocolString}:{this.ProcessId}";
                case ForwardProtocol.Tcp:
                    return $"{protocolString}:{this.Port}";
                default:
                    return $"{protocolString}:{this.SocketName}";
            }
        }
    }
}
