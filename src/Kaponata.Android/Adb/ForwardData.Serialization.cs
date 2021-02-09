// <copyright file="ForwardData.Serialization.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// Contains the serialization methods for the <see cref="ForwardData"/> class.
    /// </summary>
    public partial class ForwardData
    {
        /// <summary>
        /// Creates a new instance of the <seealso cref="ForwardData"/> class by parsing
        /// a <see cref="string"/>.
        /// </summary>
        /// <param name="value">
        /// The <see cref="string"/> value to parse.
        /// </param>
        /// <returns>
        /// A <see cref="ForwardData"/> object that represents the port forwarding information
        /// contained in <paramref name="value"/>.
        /// </returns>
        public static ForwardData FromString(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            string[] parts = value.Split(' ');

            if (parts.Length != 3)
            {
                throw new ArgumentOutOfRangeException(nameof(value), $"Cannot parse {value}");
            }

            var serial = parts[0].Trim();
            var local = ForwardSpec.Parse(parts[1].Trim());
            var remote = ForwardSpec.Parse(parts[2].Trim());

            // swap if it is a reverse forward.
            if (serial.Contains("reverse"))
            {
                var r = local;
                local = remote;
                remote = r;
            }

            return new ForwardData()
            {
                SerialNumber = parts[0],
                LocalSpec = local,
                RemoteSpec = remote,
            };
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            // swap if it is a reverse forward.
            if (this.SerialNumber.Contains("reverse"))
            {
                return $"{this.SerialNumber} {this.RemoteSpec} {this.LocalSpec}";
            }
            else
            {
                return $"{this.SerialNumber} {this.LocalSpec} {this.RemoteSpec}";
            }
        }
    }
}
