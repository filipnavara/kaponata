// <copyright file="DeviceData.Serialization.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Text.RegularExpressions;

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// Contains the serialization methods for the <see cref="DeviceData"/> class.
    /// </summary>
    public partial class DeviceData
    {
        /// <summary>
        /// The regular expression used to parse the device information.
        /// </summary>
        private static readonly Regex Regex = new Regex(
            @"^(?<serial>[a-zA-Z0-9_-]+(?:\s?[\.a-zA-Z0-9_-]+)?(?:\:\d{1,})?)\s+(?<state>device|connecting|offline|unknown|bootloader|recovery|download|authorizing|unauthorized|host|no permissions)(?<message>.*?)(\s+usb:(?<usb>[^:]+))?(?:\s+product:(?<product>[^:]+))?(\s+model\:(?<model>[\S]+))?(\s+device\:(?<device>[\S]+))?(\s+features:(?<features>[^:]+))?(\s+transport_id:(?<transport_id>[^:]+))?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Parses the <c>ADB</c> device data.
        /// </summary>
        /// <param name="data">
        /// The device data retrieved from the <c>ADB</c> server.
        /// </param>
        /// <returns>
        /// A <see cref="DeviceData"/> instance reflecting the device data.
        /// </returns>
        public static DeviceData Parse(string data)
        {
            Match m = Regex.Match(data);
            if (m.Success)
            {
                return new DeviceData()
                {
                    Serial = m.Groups["serial"].Value,
                    State = ParseState(m.Groups["state"].Value),
                    Model = m.Groups["model"].Value,
                    Product = m.Groups["product"].Value,
                    Name = m.Groups["device"].Value,
                    Features = m.Groups["features"].Value,
                    Usb = m.Groups["usb"].Value,
                    TransportId = m.Groups["transport_id"].Value,
                    Message = m.Groups["message"].Value,
                };
            }
            else
            {
                throw new ArgumentException($"Invalid device list data '{data}'");
            }
        }

        /// <summary>
        /// Parses the state <see cref="ConnectionState"/>.
        /// </summary>
        /// <param name="state">
        /// The device state string.
        /// </param>
        /// <returns>
        /// The parsed <see cref="ConnectionState"/>.
        /// </returns>
        public static ConnectionState ParseState(string state)
        {
            // Default to the unknown state
            ConnectionState value;
            if (string.Equals(state, "no permissions", StringComparison.OrdinalIgnoreCase))
            {
                value = ConnectionState.NoPermissions;
            }
            else
            {
                if (!Enum.TryParse<ConnectionState>(state, true, out value))
                {
                    value = ConnectionState.Unknown;
                }
            }

            return value;
        }
    }
}
