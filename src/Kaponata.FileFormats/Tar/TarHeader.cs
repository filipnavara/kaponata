// <copyright file="TarHeader.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;

namespace Kaponata.FileFormats.Tar
{
    /// <summary>
    /// Represents the header for an individual entry in a <c>.tar</c> archive.
    /// </summary>
    /// <seealso href="https://www.gnu.org/software/tar/manual/html_node/Standard.html"/>
    public partial struct TarHeader
    {
        /// <summary>
        /// Gets or sets the name of the current file.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the file mode, providing nine bits specifying file permissions and three bits to specify the Set UID, Set GID, and Save Text (sticky) modes.
        /// </summary>
        public LinuxFileMode FileMode { get; set; }

        /// <summary>
        /// Gets or sets the numeric user ID of the file owner.
        /// </summary>
        public uint UserId { get; set; }

        /// <summary>
        /// Gets or sets the numeric group ID of the file owner.
        /// </summary>
        public uint GroupId { get; set; }

        /// <summary>
        /// Gets or sets the size of the file in bytes. Linked files are archived with this field specified as zero.
        /// </summary>
        public uint FileSize { get; set; }

        /// <summary>
        /// Gets or sets the data modification time of the file at the time it was archived.
        /// </summary>
        public DateTimeOffset LastModified { get; set; }

        /// <summary>
        /// Gets or sets the header checksum, which is a simple sum of all bytes in the header block.
        /// </summary>
        public uint Checksum { get; set; }

        /// <summary>
        /// Gets or sets the the type of file archived.
        /// </summary>
        public TarTypeFlag TypeFlag { get; set; }

        /// <summary>
        /// Gets or sets, when <see cref="TypeFlag"/> is <see cref="TarTypeFlag.LnkType"/>, the name of the linked-to file.
        /// </summary>
        public string LinkName { get; set; }

        /// <summary>
        /// Gets or sets the header magic.
        /// </summary>
        public string Magic { get; set; }

        /// <summary>
        /// Gets or sets the version of the tar header being used.
        /// </summary>
        public uint? Version { get; set; }

        /// <summary>
        /// Gets or sets the user name of the file owner.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the group name of the file owner.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the major device number.
        /// </summary>
        public uint? DevMajor { get; set; }

        /// <summary>
        /// Gets or sets the minor device number.
        /// </summary>
        public uint? DevMinor { get; set; }

        /// <summary>
        /// Gets or sets reserved space.
        /// </summary>
        public string Prefix { get; set; }
    }
}
