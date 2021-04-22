// <copyright file="LzmaException.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;

namespace Packaging.Targets.IO
{
    /// <summary>
    /// The exception which is thrown when an LZMA error occurs.
    /// </summary>
    [Serializable]
    public class LzmaException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LzmaException"/> class.
        /// </summary>
        public LzmaException()
            : base("An LZMA error occurred.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LzmaException"/> class with a user-defined
        /// error message.
        /// </summary>
        /// <param name="message">
        /// A message which describes the error.
        /// </param>
        public LzmaException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LzmaException"/> class, based on an <see cref="LzmaResult"/>
        /// error code.
        /// </summary>
        /// <param name="result">
        /// A message which describes the error.
        /// </param>
        public LzmaException(LzmaResult result)
            : base(GetDescription(result))
        {
            this.HResult = (int)result;
        }

        /// <summary>
        /// Throws a <see cref="LzmaException"/> if <see cref="LzmaResult"/> is not <see cref="LzmaResult.OK"/>.
        /// </summary>
        /// <param name="result">
        /// The result value to check.
        /// </param>
        public static void ThrowOnError(LzmaResult result)
        {
            if (result != LzmaResult.OK)
            {
                throw new LzmaException(result);
            }
        }

        private static string GetDescription(LzmaResult result) =>
            result switch
            {
                LzmaResult.MemError => "Memory allocation failed.",
                LzmaResult.OptionsError => "Invalid or unsupported options.",
                LzmaResult.FormatError => "The input is not in the .xz format.",
                LzmaResult.DataError => "Compressed file is corrupt or file size limits exceeded.",
                LzmaResult.UnsupportedCheck => "Specified integrity check is not supported",
                LzmaResult.BufferError => "Compressed file is truncated or otherwise corrupt.",
                _ => $"An unknown LZMA error occurred: {result}.",
            };
    }
}
