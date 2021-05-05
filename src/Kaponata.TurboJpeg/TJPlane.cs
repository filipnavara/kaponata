// <copyright file="TJPlane.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.TurboJpeg
{
    /// <summary>
    /// Represents an YUV image plane.
    /// </summary>
    public enum TJPlane : int
    {
        /// <summary>
        /// The Y plane.
        /// </summary>
        Y = 0,

        /// <summary>
        /// The U/Cb plane.
        /// </summary>
        U = 1,

        /// <summary>
        /// The V/Cr plane.
        /// </summary>
        V = 2,
    }
}
