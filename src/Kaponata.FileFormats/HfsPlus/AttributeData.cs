// <copyright file="AttributeData.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using DiscUtils.Streams;
using System;

namespace DiscUtils.HfsPlus
{
    /// <summary>
    /// The base class for a node in the attributes file.
    /// </summary>
    /// <seealso href="https://developer.apple.com/library/archive/technotes/tn/tn1150.html#AttributesFile"/>
    public abstract class AttributeData : IByteArraySerializable
    {
        /// <summary>
        /// Gets or sets the attribute data record type.
        /// </summary>
        public AttributeRecordType RecordType { get; set; }

        /// <inheritdoc/>
        public abstract int Size { get; }

        /// <inheritdoc/>
        public virtual int ReadFrom(byte[] buffer, int offset)
        {
            this.RecordType = (AttributeRecordType)EndianUtilities.ToUInt32BigEndian(buffer, offset + 0);
            return 8;
        }

        /// <inheritdoc/>
        public virtual void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }
    }
}
