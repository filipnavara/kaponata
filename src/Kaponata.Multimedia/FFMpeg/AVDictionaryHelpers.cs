// <copyright file="AVDictionaryHelpers.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Kaponata.Multimedia.FFMpeg
{
    /// <summary>
    /// Provides helper methods for working with <see cref="AVDictionary"/> objects.
    /// </summary>
    public static class AVDictionaryHelpers
    {
        /// <summary>
        /// Converst a <see cref="AVDictionary"/> object to a <see cref="IReadOnlyDictionary{String, String}"/>.
        /// </summary>
        /// <param name="dictionary">
        /// The dictionary to convert.
        /// </param>
        /// <returns>
        /// An equivalent dictionary.
        /// </returns>
        public static unsafe IReadOnlyDictionary<string, string> ToReadOnlyDictionary(AVDictionary* dictionary)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            Dictionary<string, string> values = new Dictionary<string, string>();

            AVDictionaryEntry* tag = null;
            while ((tag = ffmpeg.av_dict_get(dictionary, string.Empty, tag, ffmpeg.AV_DICT_IGNORE_SUFFIX)) != null)
            {
                var key = Marshal.PtrToStringAnsi((IntPtr)tag->key);
                var value = Marshal.PtrToStringAnsi((IntPtr)tag->value);

                values.Add(key!, value!);
            }

            return values;
        }
    }
}
