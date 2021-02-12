// <copyright file="FFMpegClient.Libraries.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using FFmpeg.AutoGen;
using FFmpeg.AutoGen.Native;
using FFmpeg.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Kaponata.Multimedia.FFMpeg
{
    /// <summary>
    /// Contains the methods used to load the ffmpeg libraries.
    /// </summary>
    public partial class FFMpegClient
    {
        /// <summary>
        /// Tracks whether the libraries have already been loaded.
        /// </summary>
        private static bool librariesInitialized = false;

        /// <summary>
        /// Gets the handles of the native FFmpeg libraries loaded into memory. The key is the name of the
        /// library, without any prefix or suffix - e.g. <c>avutil</c>.
        /// </summary>
        public static Dictionary<string, IntPtr> LibraryHandles { get; } = new Dictionary<string, IntPtr>();

        /// <summary>
        /// Initializes static members of the <see cref="FFMpegClient"/> class.
        /// </summary>
        public static void Initialize()
        {
            if (librariesInitialized)
            {
                return;
            }

            ffmpeg.GetOrLoadLibrary = GetOrLoadLibrary;

#pragma warning disable CS0618 // Type or member is obsolete
            ffmpeg.av_register_all();
#pragma warning restore CS0618 // Type or member is obsolete

            librariesInitialized = true;
        }

        /// <summary>
        /// Returns the library path.
        /// </summary>
        /// <param name="name">
        /// The name of the library.
        /// </param>
        /// <param name="isWindows">
        /// A value indicating whether the os platform is windows.
        /// </param>
        /// <returns>
        /// The library path for the os platfrom.
        /// </returns>
        public static string GetNativePath(string name, bool isWindows) => (name, isWindows) switch
        {
            ("libwinpthread", true) => Path.GetFullPath(FFmpegBinaries.FindFFmpegLibrary("libwinpthread", 1)),
            (_, true) => Path.GetFullPath(FFmpegBinaries.FindFFmpegLibrary(name)),
            (_, false) => $"lib{name}.so",
        };

        /// <summary>
        /// Loads a native library into memory, and returns a handle to that library.
        /// </summary>
        /// <param name="libraryName">
        /// The name of the library to load.
        /// </param>
        /// <returns>
        /// A handle to the native library, or a null pointer on error.
        /// </returns>
        public static IntPtr GetOrLoadLibrary(string libraryName)
        {
            return GetOrLoadLibrary(libraryName, (path) => GetNativePath(path, RuntimeInformation.IsOSPlatform(OSPlatform.Windows)), (path) => NativeLibrary.Load(path));
        }

        /// <summary>
        /// Loads a native library into memory, and returns a handle to that library.
        /// </summary>
        /// <param name="libraryName">
        /// The name of the library to load.
        /// </param>
        /// <param name="getNativePath">
        /// Function to get the native path.
        /// </param>
        /// <param name="loadLibrary">
        /// The function to load the ffmpeg library.
        /// </param>
        /// <returns>
        /// A handle to the native library, or a null pointer on error.
        /// </returns>
        public static IntPtr GetOrLoadLibrary(string libraryName, Func<string, string> getNativePath, Func<string, IntPtr> loadLibrary)
        {
            lock (LibraryHandles)
            {
                if (LibraryHandles.ContainsKey(libraryName))
                {
                    return LibraryHandles[libraryName];
                }

                var libraryHandle = loadLibrary(getNativePath(libraryName));
                LibraryHandles.Add(libraryName, libraryHandle);
                return libraryHandle;
            }
        }
    }
}
