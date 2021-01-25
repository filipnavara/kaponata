// <copyright file="AdbResponse.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// Represents the response message send by the <c>ADB</c> server.
    /// </summary>
    public struct AdbResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdbResponse"/> struct.
        /// </summary>
        /// <param name="status">
        /// The <see cref="AdbResponseStatus"/> of the response.
        /// </param>
        /// <param name="message">
        /// The message in case of an <c>ADB</c> server FAIL response.
        /// </param>
        public AdbResponse(AdbResponseStatus status, string message)
        {
            this.Status = status;
            this.Message = message;
        }

        /// <summary>
        /// Gets the OKAY response.
        /// </summary>
        public static AdbResponse Success { get; } = new AdbResponse(AdbResponseStatus.OKAY, string.Empty);

        /// <summary>
        /// Gets or sets a value indicating whether the <c>ADB</c> server responded OKAY or FAIL.
        /// </summary>
        public AdbResponseStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the message in case of an <c>ADB</c> server FAIL response.
        /// </summary>
        public string Message { get; set; }
    }
}
