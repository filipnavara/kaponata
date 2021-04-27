//-----------------------------------------------------------------------
// <copyright file="XarFileEntry.cs" company="Quamotion bv">
//     Copyright (c) 2016 Quamotion. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

#nullable disable

namespace Kaponata.FileFormats.Xar
{
    /// <summary>
    /// Represents a file inside a xar archive.
    /// </summary>
    public class XarFileEntry
    {
        /// <summary>
        /// The <see cref="XElement"/> in the TOC of the XAR file which contains information about
        /// the entry.
        /// </summary>
        private readonly XElement element;

        /// <summary>
        /// Initializes a new instance of the <see cref="XarFileEntry"/> class.
        /// </summary>
        /// <param name="element">
        /// The <see cref="XElement"/> in the TOC of the XAR file which contains information about
        /// the entry.
        /// </param>
        public XarFileEntry(XElement element)
        {
            this.element = element ?? throw new ArgumentNullException(nameof(element));
        }

        /// <summary>
        /// Gets an Id which uniquely identifies the entry.
        /// </summary>
        public int Id => (int)this.element.Attribute("id");

        /// <summary>
        /// Gets the date and time at which the file was created.
        /// </summary>
        public DateTimeOffset Created => (DateTimeOffset)this.element.Element("ctime");

        /// <summary>
        /// Gets the date at which the file was modified.
        /// </summary>
        public DateTimeOffset Modified => (DateTimeOffset)this.element.Element("mtime");

        /// <summary>
        /// Gets the date at which the file as archived.
        /// </summary>
        public DateTimeOffset Archived => (DateTimeOffset)this.element.Element("atime");

        /// <summary>
        /// Gets the name of the group which owns the file.
        /// </summary>
        public string Group => (string)this.element.Element("group");

        /// <summary>
        /// Gets the ID of the group which owns the file.
        /// </summary>
        public int GroupId => (int)this.element.Element("gid");

        /// <summary>
        /// Gets the name of the user which owns the file.
        /// </summary>
        public string User => (string)this.element.Element("user");

        /// <summary>
        /// Gets the ID of the user which owns the file.
        /// </summary>
        public int UserId => (int)this.element.Element("uid");

        /// <summary>
        /// Gets the file mode flags of the file.
        /// </summary>
        public LinuxFileMode FileMode => (LinuxFileMode)Convert.ToInt32(this.element.Element("mode").Value, 8);

        /// <summary>
        /// Gets the device number of the file.
        /// </summary>
        public int DeviceNo => (int)this.element.Element("deviceno");

        /// <summary>
        /// Gets the inode number of the file.
        /// </summary>
        public long Inode => (long)this.element.Element("inode");

        /// <summary>
        /// Gets the name of the entry.
        /// </summary>
        public string Name => (string)this.element.Element("name");

        /// <summary>
        /// Gets the type of the entry, such as <c>file</c> for a file entry or <c>directory</c> for a directory entry.
        /// </summary>
        public XarEntryType Type => Enum.Parse<XarEntryType>((string)this.element.Element("type"), ignoreCase: true);

        /// <summary>
        /// Gets the offset of the compressed data in the XAR archive, relative to the start of the
        /// heap.
        /// </summary>
        public long DataOffset => (long)this.element.Element("data").Element("offset");

        /// <summary>
        /// Gets the size of the uncompressed data.
        /// </summary>
        public long DataSize => (long)this.element.Element("data").Element("size");

        /// <summary>
        /// Gets the length of the compressed data in the XAR archive.
        /// </summary>
        public long DataLength => (long)this.element.Element("data").Element("length");

        /// <summary>
        /// Gets the encoding used to compress the data. Currently the only supported value is
        /// <c>application/x-gzip</c> for Deflate encoding.
        /// </summary>
        public string Encoding => (string)this.element.Element("data").Element("encoding").Attribute("style");

        /// <summary>
        /// Gets the checksum of the uncompressed data.
        /// </summary>
        public string ExtractedChecksum => (string)this.element.Element("data").Element("extracted-checksum");

        /// <summary>
        /// Gets the algorithm used to calculate the checksum of the uncompressed data.
        /// </summary>
        public string ExtractedChecksumStyle => (string)this.element.Element("data").Element("extracted-checksum").Attribute("style");

        /// <summary>
        /// Gets the checksum of the compressed data.
        /// </summary>
        public string ArchivedChecksum => (string)this.element.Element("data").Element("archived-checksum");

        /// <summary>
        /// Gets the algorithm used to calculate the checksum of the compressed data.
        /// </summary>
        public string ArchivedChecksumStyle => (string)this.element.Element("data").Element("archived-checksum").Attribute("style");

        /// <summary>
        /// Gets a list of child entries.
        /// </summary>
        public List<XarFileEntry> Files => this.element.Elements("file").Select(f => new XarFileEntry(f)).ToList();

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.Name;
        }
    }
}
