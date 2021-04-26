// <copyright file="AttributeInlineData.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.Streams;
using System.IO;

namespace DiscUtils.HfsPlus
{
    /// <summary>
    /// A node in the attributes file which contains inline data.
    /// </summary>
    /// <seealso href="https://github.com/mirror/vbox/blob/b9657cd5351cf17432b664009cc25bb480dc64c1/include/iprt/formats/hfs.h#L517-L526"/>
    public class AttributeInlineData : AttributeData
    {
        /// <summary>
        /// Gets or sets a reserved property.
        /// </summary>
        public uint Reserved1 { get; set; }

        /// <summary>
        /// Gets or sets a reserved property.
        /// </summary>
        public uint Reserved2 { get; set; }

        /// <summary>
        /// Gets or sets the size of the data embedded in this attribute.
        /// </summary>
        public uint LogicalSize { get; set; }

        /// <inheritdoc/>
        public override int Size => 16;

        /// <inheritdoc/>
        public override int ReadFrom(byte[] buffer, int offset)
        {
            base.ReadFrom(buffer, offset);

            if (this.RecordType != AttributeRecordType.InlineData)
            {
                throw new InvalidDataException("The record is not an inline data record");
            }

            this.Reserved1 = EndianUtilities.ToUInt32BigEndian(buffer, offset + 4);
            this.Reserved1 = EndianUtilities.ToUInt32BigEndian(buffer, offset + 8);
            this.LogicalSize = EndianUtilities.ToUInt32BigEndian(buffer, offset + 12);

            return this.Size;
        }
    }
}
