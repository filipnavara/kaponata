// <copyright file="AVRational.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using NativeAVRational = FFmpeg.AutoGen.AVRational;

namespace Kaponata.Multimedia.FFMpeg
{
    /// <summary>
    /// Rational number (pair of numerator and denominator).
    /// </summary>
    public unsafe class AVRational
    {
        private readonly NativeAVRational* native;

        /// <summary>
        /// Initializes a new instance of the <see cref="AVRational"/> class.
        /// </summary>
        /// <param name="native">
        /// A pointer to the unmanaged structure which backs this <see cref="AVRational"/> class.
        /// </param>
        public AVRational(NativeAVRational* native)
        {
            this.native = native;
        }

        /// <summary>
        /// Gets the numerator.
        /// </summary>
        public int Numerator
        {
            get
            {
                return this.native->num;
            }
        }

        /// <summary>
        /// Gets the denominator.
        /// </summary>
        public int Denominator
        {
            get
            {
                return this.native->den;
            }
        }
    }
}
