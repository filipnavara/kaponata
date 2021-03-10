// <copyright file="AVInputFormat.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using NativeAVInputFormat = FFmpeg.AutoGen.AVInputFormat;

namespace Kaponata.Multimedia.FFmpeg
{
    /// <summary>
    /// Represents an audio or video input format.
    /// </summary>
    public unsafe class AVInputFormat
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AVInputFormat"/> class.
        /// </summary>
        /// <param name="nativeObject">
        /// A pointer to the unmanaged structure which backs this <see cref="AVInputFormat"/> class.
        /// </param>
        public AVInputFormat(NativeAVInputFormat* nativeObject)
        {
            this.NativeObject = nativeObject;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AVInputFormat"/> class.
        /// </summary>
        /// <param name="client">
        /// The ffmpeg client used to get access to the FFmpeg API.
        /// </param>
        /// <param name="shortName">
        /// A pointer to the unmanaged structure which backs this <see cref="AVInputFormat"/> class.
        /// </param>
        public AVInputFormat(FFmpegClient client, string shortName)
           : this((NativeAVInputFormat*)client.FindInputFormat(shortName).ToPointer())
        {
        }

        /// <summary>
        /// Gets the unmanaged structure which back this <see cref="AVInputFormat"/> class.
        /// </summary>
        public NativeAVInputFormat* NativeObject { get; }

        /// <summary>
        /// Gets a comma separated list of short names for the format.
        /// </summary>
        public string Name => new string((sbyte*)this.NativeObject->name);

        /// <summary>
        /// Gets a descriptive name for the format, meant to be more human-readable than name.
        /// </summary>
        public string LongName => new string((sbyte*)this.NativeObject->long_name);

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.LongName;
        }
    }
}
