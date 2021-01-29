// <copyright file="FileStatistics.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// Contains information about a file on the remote device.
    /// </summary>
    public class FileStatistics
    {
        /// <summary>
        /// Gets or sets the path of the file.
        /// </summary>
        public string Path
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the unix file mode of the file.
        /// </summary>
        public uint FileMode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the total file size, in bytes.
        /// </summary>
        public uint Size
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the time of last modification.
        /// </summary>
        public DateTimeOffset Time
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a <see cref="string"/> that represents the current <see cref="FileStatistics"/> object.
        /// </summary>
        /// <returns>
        /// The <see cref="Path"/> of the current <see cref="FileStatistics"/> object.
        /// </returns>
        public override string ToString()
        {
            return this.Path;
        }
    }
}
